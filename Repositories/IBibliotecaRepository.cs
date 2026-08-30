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
    Task<(List<Livro> Items, int TotalItems)> ListarLivrosAsync(
        string? termo, string? titulo, string? autor, int page, int pageSize);
    Task<Livro?> ObterLivroPorIdAsync(int id);
    Task<bool> ExisteISBNAsync(string isbn);
    Task AtualizarLivroAsync(Livro livro);
    Task<bool> LivroPossuiVinculosAsync(int livroId);
    Task RemoverLivroAsync(Livro livro);

    Task<Aluno> AdicionarAlunoAsync(Aluno aluno);
    Task<Aluno> AdicionarAlunoComUsuarioAsync(Aluno aluno, Usuario usuario);
    Task<List<Aluno>> ListarAlunosAsync();
    Task<Aluno?> ObterAlunoPorIdAsync(int id);
    Task<bool> ExisteMatriculaAsync(string matricula);
    Task<bool> ExisteEmailAsync(string email);
    Task<bool> AlunoPossuiEmprestimosAsync(int alunoId);
    Task<bool> AlunoPossuiUsuarioAsync(int alunoId);
    Task RemoverAlunoAsync(Aluno aluno);

    Task<Emprestimo> AdicionarEmprestimoAsync(Emprestimo emprestimo);
    Task<List<Emprestimo>> ListarEmprestimosAsync(int? alunoId = null);
    Task<Emprestimo?> ObterEmprestimoPorIdAsync(int id);
    Task<Emprestimo?> ObterEmprestimoPorIdComLivroAsync(int id);
    Task<bool> ExisteEmprestimoAtivoAsync(int alunoId, int livroId);
    Task AtualizarEmprestimosAtrasadosAsync(DateTime agora);
    Task<List<Emprestimo>> ListarEmprestimosParaRelatorioAsync(DateTime? dataInicio = null, DateTime? dataFim = null);

    Task<Usuario?> ObterUsuarioPorEmailAsync(string email);
    Task AdicionarUsuarioAsync(Usuario usuario);
    Task<List<Usuario>> ListarBibliotecariosAsync();
    Task<int> ContarUsuariosAsync();

    Task AdicionarAuditoriaAsync(Auditoria auditoria);
    Task<(List<Auditoria> Items, int TotalItems)> ListarAuditoriasAsync(int page, int pageSize);

    Task<Reserva> AdicionarReservaAsync(Reserva reserva);
    Task<List<Reserva>> ListarReservasAsync(int? alunoId = null);
    Task<bool> ExisteReservaAtivaAsync(int alunoId, int livroId);
    Task<List<Reserva>> ListarReservasAguardandoAsync(int livroId, int limite);
    Task<Reserva?> ObterReservaAguardandoAprovacaoAsync(int livroId, int alunoId);
    Task<Reserva?> ObterReservaPorIdAsync(int id);
    Task<int> ContarReservasAguardandoAprovacaoAsync(int livroId);
    void AdicionarNotificacao(Notificacao notificacao);
    Task<List<Notificacao>> ListarNotificacoesAsync(int alunoId);
    Task SalvarAlteracoesAsync();
}
