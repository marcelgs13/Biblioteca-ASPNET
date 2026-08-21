using BibliotecaAPI.Models;

namespace BibliotecaAPI.Repositories;

public interface IBibliotecaRepository
{
    Task<Autor> AdicionarAutorAsync(Autor autor);
    Task<List<Autor>> ListarAutoresAsync();
    Task<Autor?> ObterAutorPorIdAsync(int id);
    Task AtualizarAutorAsync(Autor autor);
    Task<bool> AutorPossuiLivrosAsync(int autorId);
    Task RemoverAutorAsync(Autor autor);

    Task<Livro> AdicionarLivroAsync(Livro livro);
    Task<List<Livro>> ListarLivrosAsync(string? titulo, string? autor);
    Task<Livro?> ObterLivroPorIdAsync(int id);
    Task<bool> ExisteISBNAsync(string isbn);
    Task AtualizarLivroAsync(Livro livro);

    Task<Aluno> AdicionarAlunoAsync(Aluno aluno);
    Task<List<Aluno>> ListarAlunosAsync();
    Task<Aluno?> ObterAlunoPorIdAsync(int id);
    Task<bool> ExisteMatriculaAsync(string matricula);
    Task<bool> ExisteEmailAsync(string email);
    Task<bool> AlunoPossuiEmprestimosAsync(int alunoId);
    Task RemoverAlunoAsync(Aluno aluno);

    Task<Emprestimo> AdicionarEmprestimoAsync(Emprestimo emprestimo);
    Task<List<Emprestimo>> ListarEmprestimosAsync();
    Task<Emprestimo?> ObterEmprestimoPorIdAsync(int id);
    Task<Emprestimo?> ObterEmprestimoPorIdComLivroAsync(int id);
    Task<bool> ExisteEmprestimoAtivoAsync(int alunoId, int livroId);
    Task SalvarAlteracoesAsync();
}
