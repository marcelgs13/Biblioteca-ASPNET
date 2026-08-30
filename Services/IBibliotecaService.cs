using BibliotecaAPI.DTOs;

namespace BibliotecaAPI.Services;

public interface IBibliotecaService
{
    Task<AutorResponseDto> CriarAutorAsync(CriarAutorDto dto);
    Task<List<AutorResponseDto>> ListarAutoresAsync();
    Task<AutorResponseDto> ObterAutorPorIdAsync(int id);
    Task<AutorResponseDto> AtualizarAutorAsync(int id, AtualizarAutorDto dto);
    Task ExcluirAutorAsync(int id);

    Task<LivroResponseDto> CriarLivroAsync(CriarLivroDto dto);
    Task<PagedResponseDto<LivroResponseDto>> ListarLivrosAsync(LivroQueryDto query);
    Task<LivroResponseDto> ObterLivroPorIdAsync(int id);
    Task<LivroResponseDto> AtualizarLivroAsync(int id, AtualizarLivroDto dto);
    Task ExcluirLivroAsync(int id);

    Task<AlunoResponseDto> CriarAlunoAsync(CriarAlunoDto dto);
    Task<List<AlunoResponseDto>> ListarAlunosAsync();
    Task<AlunoResponseDto> ObterAlunoPorIdAsync(int id);
    Task ExcluirAlunoAsync(int id);

    Task<EmprestimoResponseDto> CriarEmprestimoAsync(CriarEmprestimoDto dto);
    Task<List<EmprestimoResponseDto>> ListarEmprestimosAsync(int? alunoId = null);
    Task<EmprestimoResponseDto> ObterEmprestimoPorIdAsync(int id, int? alunoId = null);
    Task<EmprestimoResponseDto> DevolverEmprestimoAsync(int id);

    Task<List<LivroMaisEmprestadoResponseDto>> ListarLivrosMaisEmprestadosAsync();
    Task<List<UsuarioInadimplenteResponseDto>> ListarUsuariosInadimplentesAsync();
    Task<List<HistoricoEmprestimoResponseDto>> ListarHistoricoEmprestimosAsync(DateTime dataInicio, DateTime dataFim);
    Task<PagedResponseDto<AuditoriaResponseDto>> ListarAuditoriasAsync(AuditoriaQueryDto query);

    Task<ReservaResponseDto> CriarReservaAsync(int alunoId, CriarReservaDto dto);
    Task<List<ReservaResponseDto>> ListarReservasAsync(int? alunoId = null);
    Task<EmprestimoResponseDto> AprovarReservaAsync(int id);
    Task<ReservaResponseDto> RejeitarReservaAsync(int id);
    Task<ReservaResponseDto> CancelarReservaAsync(int id, int alunoId);
    Task<List<NotificacaoResponseDto>> ListarNotificacoesAsync(int alunoId);
}
