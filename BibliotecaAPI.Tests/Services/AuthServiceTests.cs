using System.IdentityModel.Tokens.Jwt;
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
using Microsoft.Extensions.Options;
using Xunit;

namespace BibliotecaAPI.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly BibliotecaDbContext _context;
    private readonly PasswordHasher<Usuario> _passwordHasher;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<BibliotecaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new BibliotecaDbContext(options);
        _context.Database.EnsureCreated();

        var repository = new BibliotecaRepository(_context);
        _passwordHasher = new PasswordHasher<Usuario>();

        var jwtOptions = Options.Create(new JwtSettings
        {
            Key = "CHAVE_SUPER_SECRETA_E_LONGA_PARA_TESTES_123456!",
            Issuer = "SmartLib",
            Audience = "SmartLib",
            ExpirationMinutes = 60
        });

        _service = new AuthService(repository, _passwordHasher, jwtOptions);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task teste_criar_bibliotecario()
    {
        // Arrange
        var dto = new CriarBibliotecarioDto { Nome = "Ana Maria", Email = "ana@bib.com", Senha = "123" };

        // Act
        var resultado = await _service.CriarBibliotecarioAsync(dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Email.Should().Be("ana@bib.com");

        var noBanco = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == "ana@bib.com");
        noBanco.Should().NotBeNull();
        noBanco!.Perfil.Should().Be(PerfilUsuario.BIBLIOTECARIO);
    }

    [Fact]
    public async Task teste_email_duplicado()
    {
        // Arrange
        var user = new Usuario { Nome = "User", Email = "duplicado@bib.com", SenhaHash = "hash", Perfil = PerfilUsuario.BIBLIOTECARIO };
        await _context.Usuarios.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var dto = new CriarBibliotecarioDto { Nome = "Outro", Email = "duplicado@bib.com", Senha = "123" };

        // Act
        var act = () => _service.CriarBibliotecarioAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ConflitoNegocioException>();
    }

    [Fact]
    public async Task teste_login_sucesso()
    {
        // Arrange
        var admin = new Usuario { Nome = "Admin", Email = "admin@bib.com", Perfil = PerfilUsuario.ADMIN };
        admin.SenhaHash = _passwordHasher.HashPassword(admin, "Senha123!");
        await _context.Usuarios.AddAsync(admin);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var dto = new LoginDto { Email = "admin@bib.com", Senha = "Senha123!" };

        // Act
        var resultado = await _service.LoginAsync(dto);

        // Assert
        resultado.Token.Should().NotBeNullOrWhiteSpace();
        new JwtSecurityTokenHandler().ReadJwtToken(resultado.Token).Claims.Should().NotBeEmpty();
    }

    [Fact]
    public async Task teste_login_senha_incorreta()
    {
        // Arrange
        var user = new Usuario { Nome = "User", Email = "user@bib.com", Perfil = PerfilUsuario.BIBLIOTECARIO };
        user.SenhaHash = _passwordHasher.HashPassword(user, "Correta123!");
        await _context.Usuarios.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var dto = new LoginDto { Email = "user@bib.com", Senha = "Errada" };

        // Act
        var act = () => _service.LoginAsync(dto);

        // Assert
        await act.Should().ThrowAsync<CredenciaisInvalidasException>();
    }

    [Fact]
    public async Task teste_listar_bibliotecarios()
    {
        // Arrange
        var bib = new Usuario { Nome = "Biblio", Email = "b@b.com", SenhaHash = "h", Perfil = PerfilUsuario.BIBLIOTECARIO };
        var aluno = new Usuario { Nome = "Aluno", Email = "a@a.com", SenhaHash = "h", Perfil = PerfilUsuario.ALUNO };
        await _context.Usuarios.AddRangeAsync(bib, aluno);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var lista = await _service.ListarBibliotecariosAsync();

        // Assert
        lista.Should().HaveCount(1);
        lista[0].Nome.Should().Be("Biblio");
    }
}
