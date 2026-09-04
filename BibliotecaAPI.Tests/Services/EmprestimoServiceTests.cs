using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Exceptions;
using BibliotecaAPI.Models;
using BibliotecaAPI.Repositories;
using BibliotecaAPI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BibliotecaAPI.Tests.Services;

public class EmprestimoServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly BibliotecaDbContext _context;
    private readonly BibliotecaService _service;

    public EmprestimoServiceTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<BibliotecaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new BibliotecaDbContext(options);
        _context.Database.EnsureCreated();

        var repository = new BibliotecaRepository(_context);
        var passwordHasher = new PasswordHasher<Usuario>();
        _service = new BibliotecaService(repository, passwordHasher);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private async Task<(Livro livro, Aluno aluno)> CriarDadosBaseAsync(int quantidade = 2)
    {
        var autor = new Autor { Nome = "Autor", DataNascimento = DateTime.UtcNow, Nacionalidade = "BR" };
        var aluno = new Aluno { Nome = "Aluno", Matricula = "MAT-" + Guid.NewGuid().ToString("N")[..6], Email = "aluno" + Guid.NewGuid().ToString("N")[..6] + "@t.com" };
        await _context.Autores.AddAsync(autor);
        await _context.Alunos.AddAsync(aluno);
        await _context.SaveChangesAsync();

        var livro = new Livro { ISBN = "ISBN-" + Guid.NewGuid().ToString("N")[..6], Titulo = "Livro", Descricao = "D", Editora = "E", Categoria = "C", Quantidade = quantidade, Localizacao = "L", AnoPublicacao = 2024, AutorId = autor.Id };
        await _context.Livros.AddAsync(livro);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        return (livro, aluno);
    }

    [Fact]
    public async Task teste_criar_emprestimo()
    {
        // Arrange
        var (livro, aluno) = await CriarDadosBaseAsync(quantidade: 3);

        // Act
        var resultado = await _service.CriarEmprestimoAsync(new CriarEmprestimoDto { AlunoId = aluno.Id, LivroId = livro.Id });

        // Assert
        resultado.Should().NotBeNull();
        resultado.Status.Should().Be(StatusEmprestimo.Ativo);

        var livroNoBanco = await _context.Livros.FindAsync(livro.Id);
        livroNoBanco!.Quantidade.Should().Be(2);
    }

    [Fact]
    public async Task teste_emprestimo_sem_estoque()
    {
        // Arrange
        var (livro, aluno) = await CriarDadosBaseAsync(quantidade: 0);

        // Act
        var act = () => _service.CriarEmprestimoAsync(new CriarEmprestimoDto { AlunoId = aluno.Id, LivroId = livro.Id });

        // Assert
        await act.Should().ThrowAsync<ConflitoNegocioException>();
    }

    [Fact]
    public async Task teste_listar_emprestimos()
    {
        // Arrange
        var (livro, aluno) = await CriarDadosBaseAsync();
        var emp = new Emprestimo { AlunoId = aluno.Id, LivroId = livro.Id, DataEmprestimo = DateTime.UtcNow, DataPrevistaDevolucao = DateTime.UtcNow.AddDays(7), Status = StatusEmprestimo.Ativo };
        await _context.Emprestimos.AddAsync(emp);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var lista = await _service.ListarEmprestimosAsync();

        // Assert
        lista.Should().HaveCount(1);
    }

    [Fact]
    public async Task teste_calculo_multa_atraso()
    {
        // Arrange
        var (livro, aluno) = await CriarDadosBaseAsync();
        var agora = DateTime.UtcNow;
        var emp = new Emprestimo
        {
            AlunoId = aluno.Id,
            LivroId = livro.Id,
            DataEmprestimo = agora.AddDays(-10),
            DataPrevistaDevolucao = agora.AddDays(-3), // 3 dias atrasado
            Status = StatusEmprestimo.Ativo
        };
        await _context.Emprestimos.AddAsync(emp);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var resultado = await _service.ObterEmprestimoPorIdAsync(emp.Id);

        // Assert
        resultado.DiasAtraso.Should().Be(3);
        resultado.Multa.Should().Be(6m); // 3 dias * R$ 2,00
    }

    [Fact]
    public async Task teste_devolver_emprestimo()
    {
        // Arrange
        var (livro, aluno) = await CriarDadosBaseAsync(quantidade: 1);
        var emp = new Emprestimo { AlunoId = aluno.Id, LivroId = livro.Id, DataEmprestimo = DateTime.UtcNow, DataPrevistaDevolucao = DateTime.UtcNow.AddDays(7), Status = StatusEmprestimo.Ativo };
        await _context.Emprestimos.AddAsync(emp);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var resultado = await _service.DevolverEmprestimoAsync(emp.Id);

        // Assert
        resultado.Status.Should().Be(StatusEmprestimo.Devolvido);
        var livroNoBanco = await _context.Livros.FindAsync(livro.Id);
        livroNoBanco!.Quantidade.Should().Be(2);
    }
}
