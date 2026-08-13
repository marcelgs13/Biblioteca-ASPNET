using BibliotecaAPI.Models;

namespace BibliotecaAPI.Repositories;

public interface IAlunoRepository
{
    Task<Aluno> AdicionarAsync(Aluno aluno);
    Task<Aluno?> ObterPorIdAsync(int id);
    Task<bool> ExisteMatriculaAsync(string matricula);
}
