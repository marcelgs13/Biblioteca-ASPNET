using BibliotecaAPI.Models;

namespace BibliotecaAPI.Repositories;

public interface IEmprestimoRepository
{
    Task<Emprestimo> AdicionarAsync(Emprestimo emprestimo);
    Task<Emprestimo?> ObterPorIdComLivroAsync(int id);
    Task<bool> ExisteAtivoAsync(int alunoId, int livroId);
    Task SalvarAlteracoesAsync();
}
