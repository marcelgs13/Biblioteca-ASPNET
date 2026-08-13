using BibliotecaAPI.Data;
using BibliotecaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Repositories;

public class LivroRepository(BibliotecaDbContext context) : ILivroRepository
{
    public async Task<Livro> AdicionarAsync(Livro livro)
    {
        await context.Livros.AddAsync(livro);
        await context.SaveChangesAsync();
        return livro;
    }

    public async Task<List<Livro>> ListarAsync(string? titulo, string? autor)
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

    public Task<Livro?> ObterPorIdAsync(int id)
    {
        return context.Livros
            .Include(livro => livro.Autor)
            .FirstOrDefaultAsync(livro => livro.Id == id);
    }
}
