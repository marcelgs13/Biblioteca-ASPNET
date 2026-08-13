using BibliotecaAPI.Data;
using BibliotecaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Repositories;

public class AutorRepository(BibliotecaDbContext context) : IAutorRepository
{
    public async Task<Autor> AdicionarAsync(Autor autor)
    {
        await context.Autores.AddAsync(autor);
        await context.SaveChangesAsync();
        return autor;
    }

    public Task<List<Autor>> ListarAsync()
    {
        return context.Autores
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<Autor?> ObterPorIdAsync(int id)
    {
        return context.Autores
            .AsNoTracking()
            .FirstOrDefaultAsync(autor => autor.Id == id);
    }
}
