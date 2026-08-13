using BibliotecaAPI.Data;
using BibliotecaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Repositories;

public class EmprestimoRepository(BibliotecaDbContext context) : IEmprestimoRepository
{
    public async Task<Emprestimo> AdicionarAsync(Emprestimo emprestimo)
    {
        await context.Emprestimos.AddAsync(emprestimo);
        await context.SaveChangesAsync();
        return emprestimo;
    }

    public Task<Emprestimo?> ObterPorIdComLivroAsync(int id)
    {
        return context.Emprestimos
            .Include(emprestimo => emprestimo.Livro)
            .FirstOrDefaultAsync(emprestimo => emprestimo.Id == id);
    }

    public Task<bool> ExisteAtivoAsync(int alunoId, int livroId)
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
