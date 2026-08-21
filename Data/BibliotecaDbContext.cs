using BibliotecaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Data;

public class BibliotecaDbContext(DbContextOptions<BibliotecaDbContext> options) : DbContext(options)
{
    public DbSet<Autor> Autores => Set<Autor>();
    public DbSet<Livro> Livros => Set<Livro>();
    public DbSet<Aluno> Alunos => Set<Aluno>();
    public DbSet<Emprestimo> Emprestimos => Set<Emprestimo>();

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
            .HasForeignKey(emprestimo => emprestimo.LivroId);

        modelBuilder.Entity<Aluno>()
            .HasIndex(aluno => aluno.Matricula)
            .IsUnique();

        modelBuilder.Entity<Aluno>()
            .HasIndex(aluno => aluno.Email)
            .IsUnique();
    }
}
