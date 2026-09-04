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

public class AlunoServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly BibliotecaDbContext _context;
    private readonly BibliotecaService _service;

    public AlunoServiceTests()
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

    [Fact]
    public async Task teste_criacao_aluno()
    {
        // Arrange
        var dto = new CriarAlunoDto { Nome = "Lucas Silva", Matricula = "MAT-100", Email = "lucas@faculdade.edu.br", Senha = "Senha123!" };

        // Act
        var resultado = await _service.CriarAlunoAsync(dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Matricula.Should().Be("MAT-100");

        var alunoNoBanco = await _context.Alunos.FindAsync(resultado.Id);
        alunoNoBanco.Should().NotBeNull();

        var usuarioNoBanco = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == "lucas@faculdade.edu.br");
        usuarioNoBanco.Should().NotBeNull();
        usuarioNoBanco!.Perfil.Should().Be(PerfilUsuario.ALUNO);
    }

    [Fact]
    public async Task teste_matricula_duplicada()
    {
        // Arrange
        var aluno = new Aluno { Nome = "Aluno 1", Matricula = "MAT-DUP", Email = "a1@teste.com" };
        await _context.Alunos.AddAsync(aluno);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var dto = new CriarAlunoDto { Nome = "Aluno 2", Matricula = "MAT-DUP", Email = "a2@teste.com", Senha = "Senha123!" };

        // Act
        var act = () => _service.CriarAlunoAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ConflitoNegocioException>();
    }

    [Fact]
    public async Task teste_listar_alunos()
    {
        // Arrange
        await _context.Alunos.AddRangeAsync(
            new Aluno { Nome = "Caio", Matricula = "MAT-C", Email = "c@teste.com" },
            new Aluno { Nome = "Bruna", Matricula = "MAT-B", Email = "b@teste.com" }
        );
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var lista = await _service.ListarAlunosAsync();

        // Assert
        lista.Should().HaveCount(2);
    }

    [Fact]
    public async Task teste_obter_aluno_por_id()
    {
        // Arrange
        var aluno = new Aluno { Nome = "Diana", Matricula = "MAT-D", Email = "diana@teste.com" };
        await _context.Alunos.AddAsync(aluno);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var resultado = await _service.ObterAlunoPorIdAsync(aluno.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Nome.Should().Be("Diana");
    }

    [Fact]
    public async Task teste_excluir_aluno()
    {
        // Arrange
        var aluno = new Aluno { Nome = "Livre", Matricula = "MAT-LIVRE", Email = "livre@teste.com" };
        await _context.Alunos.AddAsync(aluno);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        await _service.ExcluirAlunoAsync(aluno.Id);

        // Assert
        var noBanco = await _context.Alunos.FindAsync(aluno.Id);
        noBanco.Should().BeNull();
    }
}
