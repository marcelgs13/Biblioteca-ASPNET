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
    Task<List<LivroResponseDto>> ListarLivrosAsync(string? titulo, string? autor);
    Task<LivroResponseDto> ObterLivroPorIdAsync(int id);
    Task<LivroResponseDto> AtualizarLivroAsync(int id, AtualizarLivroDto dto);

    Task<AlunoResponseDto> CriarAlunoAsync(CriarAlunoDto dto);
    Task<List<AlunoResponseDto>> ListarAlunosAsync();
    Task<AlunoResponseDto> ObterAlunoPorIdAsync(int id);
    Task ExcluirAlunoAsync(int id);

    Task<EmprestimoResponseDto> CriarEmprestimoAsync(CriarEmprestimoDto dto);
    Task<List<EmprestimoResponseDto>> ListarEmprestimosAsync();
    Task<EmprestimoResponseDto> ObterEmprestimoPorIdAsync(int id);
    Task<EmprestimoResponseDto> DevolverEmprestimoAsync(int id);
}
