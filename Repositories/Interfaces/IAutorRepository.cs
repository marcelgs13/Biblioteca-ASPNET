using BibliotecaAPI.Models;

namespace BibliotecaAPI.Repositories;

public interface IAutorRepository
{
    Task<Autor> AdicionarAsync(Autor autor);
    Task<List<Autor>> ListarAsync();
    Task<Autor?> ObterPorIdAsync(int id);
}
