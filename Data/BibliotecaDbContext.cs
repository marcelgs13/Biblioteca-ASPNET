using BibliotecaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Data;

public class BibliotecaDbContext(DbContextOptions<BibliotecaDbContext> options) : DbContext(options)
{
    public DbSet<Autor> Autores => Set<Autor>();
    public DbSet<Livro> Livros => Set<Livro>();
    public DbSet<Aluno> Alunos => Set<Aluno>();
    public DbSet<Emprestimo> Emprestimos => Set<Emprestimo>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<Notificacao> Notificacoes => Set<Notificacao>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Autor>()
            .HasMany(autor => autor.Livros)
            .WithOne(livro => livro.Autor)
            .HasForeignKey(livro => livro.AutorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Aluno>()
            .HasMany(aluno => aluno.Emprestimos)
            .WithOne(emprestimo => emprestimo.Aluno)
            .HasForeignKey(emprestimo => emprestimo.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Livro>()
            .HasMany(livro => livro.Emprestimos)
            .WithOne(emprestimo => emprestimo.Livro)
            .HasForeignKey(emprestimo => emprestimo.LivroId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Usuario>()
            .HasOne(usuario => usuario.Aluno)
            .WithOne(aluno => aluno.Usuario)
            .HasForeignKey<Usuario>(usuario => usuario.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reserva>()
            .HasOne(reserva => reserva.Aluno)
            .WithMany(aluno => aluno.Reservas)
            .HasForeignKey(reserva => reserva.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reserva>()
            .HasOne(reserva => reserva.Livro)
            .WithMany(livro => livro.Reservas)
            .HasForeignKey(reserva => reserva.LivroId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Notificacao>()
            .HasOne(notificacao => notificacao.Aluno)
            .WithMany(aluno => aluno.Notificacoes)
            .HasForeignKey(notificacao => notificacao.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Aluno>()
            .HasIndex(aluno => aluno.Matricula)
            .IsUnique();

        modelBuilder.Entity<Aluno>()
            .HasIndex(aluno => aluno.Email)
            .IsUnique();

        modelBuilder.Entity<Livro>()
            .HasIndex(livro => livro.ISBN)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasIndex(usuario => usuario.Email)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasIndex(usuario => usuario.AlunoId)
            .IsUnique();

        modelBuilder.Entity<Reserva>()
            .HasIndex(reserva => new { reserva.LivroId, reserva.DataReserva });

        modelBuilder.Entity<Auditoria>()
            .HasIndex(auditoria => auditoria.Data);
    }
}
