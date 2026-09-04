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

public class ReservaServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly BibliotecaDbContext _context;
    private readonly BibliotecaService _service;

    public ReservaServiceTests()
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

    private async Task<(Livro livro, Aluno aluno)> CriarDadosBaseAsync(int quantidade = 1)
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
    public async Task teste_criar_reserva_com_estoque()
    {
        // Arrange
        var (livro, aluno) = await CriarDadosBaseAsync(quantidade: 1);

        // Act
        var resultado = await _service.CriarReservaAsync(aluno.Id, new CriarReservaDto { LivroId = livro.Id });

        // Assert
        resultado.Should().NotBeNull();
        resultado.Status.Should().Be(StatusReserva.AguardandoAprovacao);
    }

    [Fact]
    public async Task teste_criar_reserva_sem_estoque()
    {
        // Arrange
        var (livro, aluno) = await CriarDadosBaseAsync(quantidade: 0);

        // Act
        var resultado = await _service.CriarReservaAsync(aluno.Id, new CriarReservaDto { LivroId = livro.Id });

        // Assert
        resultado.Should().NotBeNull();
        resultado.Status.Should().Be(StatusReserva.AguardandoDisponibilidade);
        resultado.QuantidadeAFrente.Should().Be(0);
    }

    [Fact]
    public async Task teste_aprovar_reserva()
    {
        // Arrange
        var (livro, aluno) = await CriarDadosBaseAsync(quantidade: 1);
        var reserva = new Reserva { AlunoId = aluno.Id, LivroId = livro.Id, DataReserva = DateTime.UtcNow, Status = StatusReserva.AguardandoAprovacao };
        await _context.Reservas.AddAsync(reserva);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var emp = await _service.AprovarReservaAsync(reserva.Id);

        // Assert
        emp.Should().NotBeNull();
        emp.Status.Should().Be(StatusEmprestimo.Ativo);

        var rBanco = await _context.Reservas.FindAsync(reserva.Id);
        rBanco!.Status.Should().Be(StatusReserva.Aprovada);
    }

    [Fact]
    public async Task teste_rejeitar_reserva()
    {
        // Arrange - Aluno 1 tem a vaga, Aluno 2 na fila
        var (livro, aluno1) = await CriarDadosBaseAsync(quantidade: 1);
        var aluno2 = new Aluno { Nome = "Aluno 2", Matricula = "MAT-2", Email = "a2@teste.com" };
        await _context.Alunos.AddAsync(aluno2);
        await _context.SaveChangesAsync();

        var r1 = new Reserva { AlunoId = aluno1.Id, LivroId = livro.Id, DataReserva = DateTime.UtcNow.AddHours(-2), Status = StatusReserva.AguardandoAprovacao };
        var r2 = new Reserva { AlunoId = aluno2.Id, LivroId = livro.Id, DataReserva = DateTime.UtcNow.AddHours(-1), Status = StatusReserva.AguardandoDisponibilidade };
        await _context.Reservas.AddRangeAsync(r1, r2);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        await _service.RejeitarReservaAsync(r1.Id);

        // Assert - r2 promovida
        var r2Banco = await _context.Reservas.FindAsync(r2.Id);
        r2Banco!.Status.Should().Be(StatusReserva.AguardandoAprovacao);
    }

    [Fact]
    public async Task teste_listar_notificacoes()
    {
        // Arrange
        var (_, aluno) = await CriarDadosBaseAsync();
        var notif = new Notificacao { AlunoId = aluno.Id, Mensagem = "Livro disponível", Data = DateTime.UtcNow, Tipo = "Info" };
        await _context.Notificacoes.AddAsync(notif);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var lista = await _service.ListarNotificacoesAsync(aluno.Id);

        // Assert
        lista.Should().HaveCount(1);
        lista[0].Mensagem.Should().Be("Livro disponível");
    }
}
