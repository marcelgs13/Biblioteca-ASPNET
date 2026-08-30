using BibliotecaAPI.Data;
using BibliotecaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Repositories;

public class BibliotecaRepository(BibliotecaDbContext context) : IBibliotecaRepository
{
    public async Task<Autor> AdicionarAutorAsync(Autor autor)
    {
        await context.Autores.AddAsync(autor);
        await context.SaveChangesAsync();
        return autor;
    }

    public Task<List<Autor>> ListarAutoresAsync()
    {
        return context.Autores
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<Autor?> ObterAutorPorIdAsync(int id)
    {
        return context.Autores
            .AsNoTracking()
            .FirstOrDefaultAsync(autor => autor.Id == id);
    }

    public async Task AtualizarAutorAsync(Autor autor)
    {
        context.Autores.Update(autor);
        await context.SaveChangesAsync();
    }

    public Task<bool> AutorPossuiLivrosAsync(int autorId)
    {
        return context.Livros.AnyAsync(livro => livro.AutorId == autorId);
    }

    public async Task RemoverAutorAsync(Autor autor)
    {
        context.Autores.Remove(autor);
        await context.SaveChangesAsync();
    }

    public async Task<Livro> AdicionarLivroAsync(Livro livro)
    {
        await context.Livros.AddAsync(livro);
        await context.SaveChangesAsync();
        return livro;
    }

    public async Task<(List<Livro> Items, int TotalItems)> ListarLivrosAsync(
        string? termo, string? titulo, string? autor, int page, int pageSize)
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

        if (!string.IsNullOrWhiteSpace(termo))
        {
            var termoNormalizado = termo.Trim().ToLower();
            query = query.Where(livro =>
                livro.Titulo.ToLower().Contains(termoNormalizado) ||
                livro.ISBN.ToLower().Contains(termoNormalizado) ||
                livro.Descricao.ToLower().Contains(termoNormalizado) ||
                livro.Editora.ToLower().Contains(termoNormalizado) ||
                livro.Categoria.ToLower().Contains(termoNormalizado) ||
                livro.Localizacao.ToLower().Contains(termoNormalizado) ||
                livro.Autor.Nome.ToLower().Contains(termoNormalizado));
        }

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderBy(livro => livro.Titulo)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalItems);
    }

    public Task<Livro?> ObterLivroPorIdAsync(int id)
    {
        return context.Livros
            .Include(livro => livro.Autor)
            .FirstOrDefaultAsync(livro => livro.Id == id);
    }

    public Task<bool> ExisteISBNAsync(string isbn)
    {
        return context.Livros.AnyAsync(livro => livro.ISBN == isbn);
    }

    public async Task AtualizarLivroAsync(Livro livro)
    {
        context.Entry(livro).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }

    public async Task<bool> LivroPossuiVinculosAsync(int livroId)
    {
        if (await context.Emprestimos.AnyAsync(emprestimo => emprestimo.LivroId == livroId))
        {
            return true;
        }

        return await context.Reservas.AnyAsync(reserva => reserva.LivroId == livroId);
    }

    public async Task RemoverLivroAsync(Livro livro)
    {
        context.Livros.Remove(livro);
        await context.SaveChangesAsync();
    }

    public async Task<Aluno> AdicionarAlunoAsync(Aluno aluno)
    {
        await context.Alunos.AddAsync(aluno);
        await context.SaveChangesAsync();
        return aluno;
    }

    public async Task<Aluno> AdicionarAlunoComUsuarioAsync(Aluno aluno, Usuario usuario)
    {
        await context.Usuarios.AddAsync(usuario);
        await context.SaveChangesAsync();
        return aluno;
    }

    public Task<List<Aluno>> ListarAlunosAsync()
    {
        return context.Alunos
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<Aluno?> ObterAlunoPorIdAsync(int id)
    {
        return context.Alunos
            .AsNoTracking()
            .FirstOrDefaultAsync(aluno => aluno.Id == id);
    }

    public Task<bool> ExisteMatriculaAsync(string matricula)
    {
        return context.Alunos.AnyAsync(aluno => aluno.Matricula == matricula);
    }

    public Task<bool> ExisteEmailAsync(string email)
    {
        return context.Alunos.AnyAsync(aluno => aluno.Email.ToLower() == email);
    }

    public async Task<bool> AlunoPossuiEmprestimosAsync(int alunoId)
    {
        if (await context.Emprestimos.AnyAsync(emprestimo => emprestimo.AlunoId == alunoId)) return true;
        if (await context.Reservas.AnyAsync(reserva => reserva.AlunoId == alunoId)) return true;
        return await context.Notificacoes.AnyAsync(notificacao => notificacao.AlunoId == alunoId);
    }

    public Task<bool> AlunoPossuiUsuarioAsync(int alunoId)
    {
        return context.Usuarios.AnyAsync(usuario => usuario.AlunoId == alunoId);
    }

    public async Task RemoverAlunoAsync(Aluno aluno)
    {
        await context.Usuarios.Where(usuario => usuario.AlunoId == aluno.Id).ExecuteDeleteAsync();
        context.Alunos.Remove(aluno);
        await context.SaveChangesAsync();
    }

    public async Task<Emprestimo> AdicionarEmprestimoAsync(Emprestimo emprestimo)
    {
        await context.Emprestimos.AddAsync(emprestimo);
        await context.SaveChangesAsync();
        return emprestimo;
    }

    public Task<List<Emprestimo>> ListarEmprestimosAsync(int? alunoId = null)
    {
        var query = context.Emprestimos.AsNoTracking().AsQueryable();
        if (alunoId.HasValue)
        {
            query = query.Where(emprestimo => emprestimo.AlunoId == alunoId.Value);
        }

        return query.OrderByDescending(emprestimo => emprestimo.DataEmprestimo).ToListAsync();
    }

    public Task<Emprestimo?> ObterEmprestimoPorIdAsync(int id)
    {
        return context.Emprestimos
            .AsNoTracking()
            .FirstOrDefaultAsync(emprestimo => emprestimo.Id == id);
    }

    public Task<Emprestimo?> ObterEmprestimoPorIdComLivroAsync(int id)
    {
        return context.Emprestimos
            .Include(emprestimo => emprestimo.Livro)
            .FirstOrDefaultAsync(emprestimo => emprestimo.Id == id);
    }

    public Task<bool> ExisteEmprestimoAtivoAsync(int alunoId, int livroId)
    {
        return context.Emprestimos.AnyAsync(emprestimo =>
            emprestimo.AlunoId == alunoId &&
            emprestimo.LivroId == livroId &&
            (emprestimo.Status == StatusEmprestimo.Ativo ||
             emprestimo.Status == StatusEmprestimo.Atrasado));
    }

    public Task AtualizarEmprestimosAtrasadosAsync(DateTime agora)
    {
        return context.Emprestimos
            .Where(emprestimo =>
                emprestimo.Status == StatusEmprestimo.Ativo &&
                emprestimo.DataPrevistaDevolucao < agora)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(emprestimo => emprestimo.Status, StatusEmprestimo.Atrasado));
    }

    public Task<List<Emprestimo>> ListarEmprestimosParaRelatorioAsync(
        DateTime? dataInicio = null,
        DateTime? dataFim = null)
    {
        var query = context.Emprestimos
            .Include(emprestimo => emprestimo.Aluno)
            .Include(emprestimo => emprestimo.Livro)
                .ThenInclude(livro => livro.Autor)
            .AsNoTracking()
            .AsQueryable();

        if (dataInicio.HasValue)
        {
            query = query.Where(emprestimo => emprestimo.DataEmprestimo >= dataInicio.Value.Date);
        }

        if (dataFim.HasValue)
        {
            var limiteExclusivo = dataFim.Value.Date.AddDays(1);
            query = query.Where(emprestimo => emprestimo.DataEmprestimo < limiteExclusivo);
        }

        return query
            .OrderByDescending(emprestimo => emprestimo.DataEmprestimo)
            .ThenByDescending(emprestimo => emprestimo.Id)
            .ToListAsync();
    }

    public Task<Usuario?> ObterUsuarioPorEmailAsync(string email)
    {
        return context.Usuarios
            .Include(usuario => usuario.Aluno)
            .FirstOrDefaultAsync(usuario => usuario.Email == email);
    }

    public async Task AdicionarUsuarioAsync(Usuario usuario)
    {
        await context.Usuarios.AddAsync(usuario);
        await context.SaveChangesAsync();
    }

    public Task<List<Usuario>> ListarBibliotecariosAsync()
    {
        return context.Usuarios
            .AsNoTracking()
            .Where(usuario => usuario.Perfil == PerfilUsuario.BIBLIOTECARIO)
            .OrderBy(usuario => usuario.Nome)
            .ToListAsync();
    }

    public Task<int> ContarUsuariosAsync()
    {
        return context.Usuarios.CountAsync();
    }

    public async Task AdicionarAuditoriaAsync(Auditoria auditoria)
    {
        await context.Auditorias.AddAsync(auditoria);
        await context.SaveChangesAsync();
    }

    public async Task<(List<Auditoria> Items, int TotalItems)> ListarAuditoriasAsync(int page, int pageSize)
    {
        var query = context.Auditorias.AsNoTracking();
        var totalItems = await query.CountAsync();
        var items = await query
            .OrderByDescending(auditoria => auditoria.Data)
            .ThenByDescending(auditoria => auditoria.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalItems);
    }

    public async Task<Reserva> AdicionarReservaAsync(Reserva reserva)
    {
        await context.Reservas.AddAsync(reserva);
        await context.SaveChangesAsync();
        return reserva;
    }

    public Task<List<Reserva>> ListarReservasAsync(int? alunoId = null)
    {
        var query = context.Reservas
            .Include(reserva => reserva.Aluno)
            .Include(reserva => reserva.Livro)
            .AsNoTracking()
            .AsQueryable();

        if (alunoId.HasValue)
        {
            query = query.Where(reserva => reserva.AlunoId == alunoId.Value);
        }

        return query.OrderBy(reserva => reserva.DataReserva).ThenBy(reserva => reserva.Id).ToListAsync();
    }

    public Task<bool> ExisteReservaAtivaAsync(int alunoId, int livroId)
    {
        return context.Reservas.AnyAsync(reserva =>
            reserva.AlunoId == alunoId &&
            reserva.LivroId == livroId &&
            (reserva.Status == StatusReserva.AguardandoDisponibilidade ||
             reserva.Status == StatusReserva.AguardandoAprovacao));
    }

    public Task<List<Reserva>> ListarReservasAguardandoAsync(int livroId, int limite)
    {
        return context.Reservas
            .Include(reserva => reserva.Aluno)
            .Where(reserva => reserva.LivroId == livroId &&
                reserva.Status == StatusReserva.AguardandoDisponibilidade)
            .OrderBy(reserva => reserva.DataReserva)
            .ThenBy(reserva => reserva.Id)
            .Take(limite)
            .ToListAsync();
    }

    public Task<Reserva?> ObterReservaAguardandoAprovacaoAsync(int livroId, int alunoId)
    {
        return context.Reservas
            .Where(reserva => reserva.LivroId == livroId &&
                reserva.AlunoId == alunoId &&
                reserva.Status == StatusReserva.AguardandoAprovacao)
            .OrderBy(reserva => reserva.DataReserva)
            .ThenBy(reserva => reserva.Id)
            .FirstOrDefaultAsync();
    }

    public Task<Reserva?> ObterReservaPorIdAsync(int id)
    {
        return context.Reservas
            .Include(reserva => reserva.Aluno)
            .Include(reserva => reserva.Livro)
            .FirstOrDefaultAsync(reserva => reserva.Id == id);
    }

    public Task<int> ContarReservasAguardandoAprovacaoAsync(int livroId)
    {
        return context.Reservas.CountAsync(reserva =>
            reserva.LivroId == livroId &&
            reserva.Status == StatusReserva.AguardandoAprovacao);
    }

    public void AdicionarNotificacao(Notificacao notificacao)
    {
        context.Notificacoes.Add(notificacao);
    }

    public Task<List<Notificacao>> ListarNotificacoesAsync(int alunoId)
    {
        return context.Notificacoes
            .AsNoTracking()
            .Where(notificacao => notificacao.AlunoId == alunoId)
            .OrderByDescending(notificacao => notificacao.Data)
            .ToListAsync();
    }

    public async Task SalvarAlteracoesAsync()
    {
        await context.SaveChangesAsync();
    }
}
