using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Models;
using BibliotecaAPI.Repositories;
using BibliotecaAPI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BibliotecaAPI.Tests.Services;

public class RelatorioAuditoriaServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly BibliotecaDbContext _context;
    private readonly BibliotecaService _service;

    public RelatorioAuditoriaServiceTests()
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
    public async Task teste_livros_mais_emprestados()
    {
        // Arrange
        var autor = new Autor { Nome = "Autor", DataNascimento = DateTime.UtcNow, Nacionalidade = "BR" };
        var aluno = new Aluno { Nome = "Aluno", Matricula = "MAT-1", Email = "a@t.com" };
        await _context.Autores.AddAsync(autor);
        await _context.Alunos.AddAsync(aluno);
        await _context.SaveChangesAsync();

        var l1 = new Livro { ISBN = "1", Titulo = "Top 1", Descricao = "D", Editora = "E", Categoria = "C", Quantidade = 5, Localizacao = "L", AnoPublicacao = 2020, AutorId = autor.Id };
        var l2 = new Livro { ISBN = "2", Titulo = "Top 2", Descricao = "D", Editora = "E", Categoria = "C", Quantidade = 5, Localizacao = "L", AnoPublicacao = 2021, AutorId = autor.Id };
        await _context.Livros.AddRangeAsync(l1, l2);
        await _context.SaveChangesAsync();

        // 2 empréstimos para l1 e 1 para l2
        await _context.Emprestimos.AddRangeAsync(
            new Emprestimo { AlunoId = aluno.Id, LivroId = l1.Id, DataEmprestimo = DateTime.UtcNow, DataPrevistaDevolucao = DateTime.UtcNow.AddDays(7), Status = StatusEmprestimo.Ativo },
            new Emprestimo { AlunoId = aluno.Id, LivroId = l1.Id, DataEmprestimo = DateTime.UtcNow, DataPrevistaDevolucao = DateTime.UtcNow.AddDays(7), Status = StatusEmprestimo.Ativo },
            new Emprestimo { AlunoId = aluno.Id, LivroId = l2.Id, DataEmprestimo = DateTime.UtcNow, DataPrevistaDevolucao = DateTime.UtcNow.AddDays(7), Status = StatusEmprestimo.Ativo }
        );
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var lista = await _service.ListarLivrosMaisEmprestadosAsync();

        // Assert
        lista.Should().HaveCount(2);
        lista[0].Titulo.Should().Be("Top 1");
        lista[0].QuantidadeEmprestimos.Should().Be(2);
    }

    [Fact]
    public async Task teste_usuarios_inadimplentes()
    {
        // Arrange
        var autor = new Autor { Nome = "Autor", DataNascimento = DateTime.UtcNow, Nacionalidade = "BR" };
        var aluno = new Aluno { Nome = "Devedor", Matricula = "MAT-DEV", Email = "dev@t.com" };
        await _context.Autores.AddAsync(autor);
        await _context.Alunos.AddAsync(aluno);
        await _context.SaveChangesAsync();

        var livro = new Livro { ISBN = "1", Titulo = "Livro", Descricao = "D", Editora = "E", Categoria = "C", Quantidade = 5, Localizacao = "L", AnoPublicacao = 2020, AutorId = autor.Id };
        await _context.Livros.AddAsync(livro);
        await _context.SaveChangesAsync();

        var agora = DateTime.UtcNow;
        var emp = new Emprestimo
        {
            AlunoId = aluno.Id,
            LivroId = livro.Id,
            DataEmprestimo = agora.AddDays(-10),
            DataPrevistaDevolucao = agora.AddDays(-3), // 3 dias atrasado -> R$ 6,00
            Status = StatusEmprestimo.Ativo
        };
        await _context.Emprestimos.AddAsync(emp);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var lista = await _service.ListarUsuariosInadimplentesAsync();

        // Assert
        lista.Should().HaveCount(1);
        lista[0].Nome.Should().Be("Devedor");
        lista[0].DiasAtrasoTotal.Should().Be(3);
        lista[0].MultaTotal.Should().Be(6m);
    }

    [Fact]
    public async Task teste_historico_por_periodo()
    {
        // Arrange
        var autor = new Autor { Nome = "Autor", DataNascimento = DateTime.UtcNow, Nacionalidade = "BR" };
        var aluno = new Aluno { Nome = "Aluno", Matricula = "MAT-H", Email = "h@t.com" };
        await _context.Autores.AddAsync(autor);
        await _context.Alunos.AddAsync(aluno);
        await _context.SaveChangesAsync();

        var livro = new Livro { ISBN = "1", Titulo = "Livro", Descricao = "D", Editora = "E", Categoria = "C", Quantidade = 5, Localizacao = "L", AnoPublicacao = 2020, AutorId = autor.Id };
        await _context.Livros.AddAsync(livro);
        await _context.SaveChangesAsync();

        await _context.Emprestimos.AddRangeAsync(
            new Emprestimo { AlunoId = aluno.Id, LivroId = livro.Id, DataEmprestimo = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc), DataPrevistaDevolucao = DateTime.UtcNow, Status = StatusEmprestimo.Ativo },
            new Emprestimo { AlunoId = aluno.Id, LivroId = livro.Id, DataEmprestimo = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), DataPrevistaDevolucao = DateTime.UtcNow, Status = StatusEmprestimo.Ativo }
        );
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var lista = await _service.ListarHistoricoEmprestimosAsync(new DateTime(2026, 1, 10), new DateTime(2026, 1, 20));

        // Assert
        lista.Should().HaveCount(1);
        lista[0].DataEmprestimo.Should().Be(new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task teste_listar_auditorias_paginadas()
    {
        // Arrange
        await _context.Auditorias.AddRangeAsync(
            new Auditoria { UsuarioId = 1, UsuarioNome = "Admin", Perfil = "ADMIN", Acao = "CREATE", Detalhes = "D1", Data = DateTime.UtcNow },
            new Auditoria { UsuarioId = 1, UsuarioNome = "Admin", Perfil = "ADMIN", Acao = "UPDATE", Detalhes = "D2", Data = DateTime.UtcNow },
            new Auditoria { UsuarioId = 1, UsuarioNome = "Admin", Perfil = "ADMIN", Acao = "DELETE", Detalhes = "D3", Data = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var resultado = await _service.ListarAuditoriasAsync(new AuditoriaQueryDto { Page = 1, PageSize = 2 });

        // Assert
        resultado.TotalItems.Should().Be(3);
        resultado.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task teste_listar_auditorias_vazio()
    {
        // Act
        var resultado = await _service.ListarAuditoriasAsync(new AuditoriaQueryDto { Page = 1, PageSize = 10 });

        // Assert
        resultado.TotalItems.Should().Be(0);
        resultado.Items.Should().BeEmpty();
    }
}
