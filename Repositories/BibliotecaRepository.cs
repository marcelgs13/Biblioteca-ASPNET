using BibliotecaAPI.Data;
using BibliotecaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Repositories;

public class BibliotecaRepository(BibliotecaDbContext context) : IBibliotecaRepository
{
    public async Task<Autor> AdicionarAutorAsync(Autor autor)
    {
        await context.Autores.AddAsync(autor);
        await context.SaveChangesAsync();
        return autor;
    }

    public Task<List<Autor>> ListarAutoresAsync()
    {
        return context.Autores
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<Autor?> ObterAutorPorIdAsync(int id)
    {
        return context.Autores
            .AsNoTracking()
            .FirstOrDefaultAsync(autor => autor.Id == id);
    }

    public async Task AtualizarAutorAsync(Autor autor)
    {
        context.Autores.Update(autor);
        await context.SaveChangesAsync();
    }

    public Task<bool> AutorPossuiLivrosAsync(int autorId)
    {
        return context.Livros.AnyAsync(livro => livro.AutorId == autorId);
    }

    public async Task RemoverAutorAsync(Autor autor)
    {
        context.Autores.Remove(autor);
        await context.SaveChangesAsync();
    }

    public async Task<Livro> AdicionarLivroAsync(Livro livro)
    {
        await context.Livros.AddAsync(livro);
        await context.SaveChangesAsync();
        return livro;
    }

    public async Task<List<Livro>> ListarLivrosAsync(string? titulo, string? autor)
    {
        var query = context.Livros
            .Include(livro => livro.Autor)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(titulo))
        {
            var tituloNormalizado = titulo.Trim().ToLower();
            query = query.Where(livro => livro.Titulo.ToLower().Contains(tituloNormalizado));
        }

        if (!string.IsNullOrWhiteSpace(autor))
        {
            var autorNormalizado = autor.Trim().ToLower();
            query = query.Where(livro => livro.Autor.Nome.ToLower().Contains(autorNormalizado));
        }

        return await query.ToListAsync();
    }

    public Task<Livro?> ObterLivroPorIdAsync(int id)
    {
        return context.Livros
            .Include(livro => livro.Autor)
            .FirstOrDefaultAsync(livro => livro.Id == id);
    }

    public async Task<Aluno> AdicionarAlunoAsync(Aluno aluno)
    {
        await context.Alunos.AddAsync(aluno);
        await context.SaveChangesAsync();
        return aluno;
    }

    public Task<List<Aluno>> ListarAlunosAsync()
    {
        return context.Alunos
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<Aluno?> ObterAlunoPorIdAsync(int id)
    {
        return context.Alunos
            .AsNoTracking()
            .FirstOrDefaultAsync(aluno => aluno.Id == id);
    }

    public Task<bool> ExisteMatriculaAsync(string matricula)
    {
        return context.Alunos.AnyAsync(aluno => aluno.Matricula == matricula);
    }

    public Task<bool> AlunoPossuiEmprestimosAsync(int alunoId)
    {
        return context.Emprestimos.AnyAsync(emprestimo => emprestimo.AlunoId == alunoId);
    }

    public async Task RemoverAlunoAsync(Aluno aluno)
    {
        context.Alunos.Remove(aluno);
        await context.SaveChangesAsync();
    }

    public async Task<Emprestimo> AdicionarEmprestimoAsync(Emprestimo emprestimo)
    {
        await context.Emprestimos.AddAsync(emprestimo);
        await context.SaveChangesAsync();
        return emprestimo;
    }

    public Task<List<Emprestimo>> ListarEmprestimosAsync()
    {
        return context.Emprestimos
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<Emprestimo?> ObterEmprestimoPorIdAsync(int id)
    {
        return context.Emprestimos
            .AsNoTracking()
            .FirstOrDefaultAsync(emprestimo => emprestimo.Id == id);
    }

    public Task<Emprestimo?> ObterEmprestimoPorIdComLivroAsync(int id)
    {
        return context.Emprestimos
            .Include(emprestimo => emprestimo.Livro)
            .FirstOrDefaultAsync(emprestimo => emprestimo.Id == id);
    }

    public Task<bool> ExisteEmprestimoAtivoAsync(int alunoId, int livroId)
    {
        return context.Emprestimos.AnyAsync(emprestimo =>
            emprestimo.AlunoId == alunoId &&
            emprestimo.LivroId == livroId &&
            emprestimo.Status == StatusEmprestimo.Ativo);
    }

    public async Task SalvarAlteracoesAsync()
    {
        await context.SaveChangesAsync();
    }
}
