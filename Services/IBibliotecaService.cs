using BibliotecaAPI.DTOs;

namespace BibliotecaAPI.Services;

public interface IBibliotecaService
{
    Task<AutorResponseDto> CriarAutorAsync(CriarAutorDto dto);
    Task<List<AutorResponseDto>> ListarAutoresAsync();
    Task<AutorResponseDto> ObterAutorPorIdAsync(int id);

    Task<LivroResponseDto> CriarLivroAsync(CriarLivroDto dto);
    Task<List<LivroResponseDto>> ListarLivrosAsync(string? titulo, string? autor);
    Task<LivroResponseDto> ObterLivroPorIdAsync(int id);

    Task<AlunoResponseDto> CriarAlunoAsync(CriarAlunoDto dto);

    Task<EmprestimoResponseDto> CriarEmprestimoAsync(CriarEmprestimoDto dto);
    Task<EmprestimoResponseDto> DevolverEmprestimoAsync(int id);
}
