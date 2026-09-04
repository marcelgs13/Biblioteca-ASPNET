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

public class AutorServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly BibliotecaDbContext _context;
    private readonly BibliotecaService _service;

    public AutorServiceTests()
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
    public async Task teste_criar_autor()
    {
        // Arrange
        var dto = new CriarAutorDto { Nome = "Machado de Assis", DataNascimento = new DateTime(1839, 6, 21), Nacionalidade = "Brasileira" };

        // Act
        var resultado = await _service.CriarAutorAsync(dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Nome.Should().Be("Machado de Assis");

        var noBanco = await _context.Autores.FindAsync(resultado.Id);
        noBanco.Should().NotBeNull();
    }

    [Fact]
    public async Task teste_listar_autores()
    {
        // Arrange
        await _context.Autores.AddRangeAsync(
            new Autor { Nome = "Autor 1", DataNascimento = DateTime.UtcNow, Nacionalidade = "BR" },
            new Autor { Nome = "Autor 2", DataNascimento = DateTime.UtcNow, Nacionalidade = "BR" }
        );
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var lista = await _service.ListarAutoresAsync();

        // Assert
        lista.Should().HaveCount(2);
    }

    [Fact]
    public async Task teste_obter_autor_por_id()
    {
        // Arrange
        var autor = new Autor { Nome = "Clarice Lispector", DataNascimento = DateTime.UtcNow, Nacionalidade = "BR" };
        await _context.Autores.AddAsync(autor);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var resultado = await _service.ObterAutorPorIdAsync(autor.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Nome.Should().Be("Clarice Lispector");
    }

    [Fact]
    public async Task teste_atualizar_autor()
    {
        // Arrange
        var autor = new Autor { Nome = "Nome Antigo", DataNascimento = DateTime.UtcNow, Nacionalidade = "BR" };
        await _context.Autores.AddAsync(autor);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var dto = new AtualizarAutorDto { Nome = "Nome Novo", DataNascimento = DateTime.UtcNow, Nacionalidade = "BR" };

        // Act
        var resultado = await _service.AtualizarAutorAsync(autor.Id, dto);

        // Assert
        resultado.Nome.Should().Be("Nome Novo");
        var noBanco = await _context.Autores.FindAsync(autor.Id);
        noBanco!.Nome.Should().Be("Nome Novo");
    }

    [Fact]
    public async Task teste_excluir_autor_com_livros()
    {
        // Arrange
        var autor = new Autor { Nome = "Autor com Livro", DataNascimento = DateTime.UtcNow, Nacionalidade = "BR" };
        await _context.Autores.AddAsync(autor);
        await _context.SaveChangesAsync();

        var livro = new Livro { ISBN = "123", Titulo = "Livro", Descricao = "D", Editora = "E", Categoria = "C", Quantidade = 1, Localizacao = "L", AnoPublicacao = 2020, AutorId = autor.Id };
        await _context.Livros.AddAsync(livro);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var act = () => _service.ExcluirAutorAsync(autor.Id);

        // Assert
        await act.Should().ThrowAsync<ConflitoNegocioException>();
    }
}
