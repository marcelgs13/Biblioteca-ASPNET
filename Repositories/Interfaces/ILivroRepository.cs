using BibliotecaAPI.Models;

namespace BibliotecaAPI.Repositories;

public interface ILivroRepository
{
    Task<Livro> AdicionarAsync(Livro livro);
    Task<List<Livro>> ListarAsync(string? titulo, string? autor);
    Task<Livro?> ObterPorIdAsync(int id);
}
