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

public class LivroServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly BibliotecaDbContext _context;
    private readonly BibliotecaService _service;

    public LivroServiceTests()
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

    private async Task<Autor> CriarAutorAsync()
    {
        var autor = new Autor { Nome = "Autor Teste", DataNascimento = DateTime.UtcNow, Nacionalidade = "BR" };
        await _context.Autores.AddAsync(autor);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        return autor;
    }

    [Fact]
    public async Task teste_criar_livro()
    {
        // Arrange
        var autor = await CriarAutorAsync();
        var dto = new CriarLivroDto
        {
            ISBN = "978-85-359-0277-1",
            Titulo = "O Senhor dos Anéis",
            Descricao = "Fantasia",
            AnoPublicacao = 1954,
            Editora = "HarperCollins",
            Categoria = "Fantasia",
            Quantidade = 5,
            Localizacao = "Estante A",
            AutorId = autor.Id
        };

        // Act
        var resultado = await _service.CriarLivroAsync(dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Titulo.Should().Be("O Senhor dos Anéis");
        resultado.AutorNome.Should().Be("Autor Teste");

        var noBanco = await _context.Livros.FindAsync(resultado.Id);
        noBanco.Should().NotBeNull();
    }

    [Fact]
    public async Task teste_isbn_duplicado()
    {
        // Arrange
        var autor = await CriarAutorAsync();
        var livroExistente = new Livro { ISBN = "111-222", Titulo = "Livro 1", Descricao = "D", Editora = "E", Categoria = "C", Quantidade = 1, Localizacao = "L", AnoPublicacao = 2020, AutorId = autor.Id };
        await _context.Livros.AddAsync(livroExistente);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var dto = new CriarLivroDto { ISBN = "111-222", Titulo = "Livro 2", Descricao = "D", Editora = "E", Categoria = "C", Quantidade = 1, Localizacao = "L", AnoPublicacao = 2021, AutorId = autor.Id };

        // Act
        var act = () => _service.CriarLivroAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ConflitoNegocioException>();
    }

    [Fact]
    public async Task teste_listar_livros()
    {
        // Arrange
        var autor = await CriarAutorAsync();
        await _context.Livros.AddRangeAsync(
            new Livro { ISBN = "1", Titulo = "Livro A", Descricao = "D", Editora = "E", Categoria = "C", Quantidade = 1, Localizacao = "L", AnoPublicacao = 2020, AutorId = autor.Id },
            new Livro { ISBN = "2", Titulo = "Livro B", Descricao = "D", Editora = "E", Categoria = "C", Quantidade = 1, Localizacao = "L", AnoPublicacao = 2021, AutorId = autor.Id }
        );
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var resultado = await _service.ListarLivrosAsync(new LivroQueryDto { Page = 1, PageSize = 10 });

        // Assert
        resultado.TotalItems.Should().Be(2);
        resultado.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task teste_obter_livro_por_id()
    {
        // Arrange
        var autor = await CriarAutorAsync();
        var livro = new Livro { ISBN = "999", Titulo = "Livro Encontrado", Descricao = "D", Editora = "E", Categoria = "C", Quantidade = 1, Localizacao = "L", AnoPublicacao = 2020, AutorId = autor.Id };
        await _context.Livros.AddAsync(livro);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var resultado = await _service.ObterLivroPorIdAsync(livro.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Titulo.Should().Be("Livro Encontrado");
    }

    [Fact]
    public async Task teste_excluir_livro_com_emprestimo()
    {
        // Arrange
        var autor = await CriarAutorAsync();
        var aluno = new Aluno { Nome = "Aluno", Matricula = "MAT-1", Email = "aluno@teste.com" };
        await _context.Alunos.AddAsync(aluno);
        await _context.SaveChangesAsync();

        var livro = new Livro { ISBN = "555", Titulo = "Livro Emprestado", Descricao = "D", Editora = "E", Categoria = "C", Quantidade = 1, Localizacao = "L", AnoPublicacao = 2020, AutorId = autor.Id };
        await _context.Livros.AddAsync(livro);
        await _context.SaveChangesAsync();

        var emp = new Emprestimo { AlunoId = aluno.Id, LivroId = livro.Id, DataEmprestimo = DateTime.UtcNow, DataPrevistaDevolucao = DateTime.UtcNow.AddDays(7), Status = StatusEmprestimo.Ativo };
        await _context.Emprestimos.AddAsync(emp);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var act = () => _service.ExcluirLivroAsync(livro.Id);

        // Assert
        await act.Should().ThrowAsync<ConflitoNegocioException>();
    }
}
