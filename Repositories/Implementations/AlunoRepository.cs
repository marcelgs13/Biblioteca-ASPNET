using BibliotecaAPI.Data;
using BibliotecaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Repositories;

public class AlunoRepository(BibliotecaDbContext context) : IAlunoRepository
{
    public async Task<Aluno> AdicionarAsync(Aluno aluno)
    {
        await context.Alunos.AddAsync(aluno);
        await context.SaveChangesAsync();
        return aluno;
    }

    public Task<Aluno?> ObterPorIdAsync(int id)
    {
        return context.Alunos
            .AsNoTracking()
            .FirstOrDefaultAsync(aluno => aluno.Id == id);
    }

    public Task<bool> ExisteMatriculaAsync(string matricula)
    {
        return context.Alunos.AnyAsync(aluno => aluno.Matricula == matricula);
    }
}
