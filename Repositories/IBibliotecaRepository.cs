using BibliotecaAPI.Models;

namespace BibliotecaAPI.Repositories;

public interface IBibliotecaRepository
{
    Task<Autor> AdicionarAutorAsync(Autor autor);
    Task<List<Autor>> ListarAutoresAsync();
    Task<Autor?> ObterAutorPorIdAsync(int id);

    Task<Livro> AdicionarLivroAsync(Livro livro);
    Task<List<Livro>> ListarLivrosAsync(string? titulo, string? autor);
    Task<Livro?> ObterLivroPorIdAsync(int id);

    Task<Aluno> AdicionarAlunoAsync(Aluno aluno);
    Task<Aluno?> ObterAlunoPorIdAsync(int id);
    Task<bool> ExisteMatriculaAsync(string matricula);

    Task<Emprestimo> AdicionarEmprestimoAsync(Emprestimo emprestimo);
    Task<Emprestimo?> ObterEmprestimoPorIdComLivroAsync(int id);
    Task<bool> ExisteEmprestimoAtivoAsync(int alunoId, int livroId);
    Task SalvarAlteracoesAsync();
}
