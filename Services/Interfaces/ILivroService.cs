using BibliotecaAPI.DTOs;

namespace BibliotecaAPI.Services;

public interface ILivroService
{
    Task<LivroResponseDto> CriarAsync(CriarLivroDto dto);
    Task<List<LivroResponseDto>> ListarAsync(string? titulo, string? autor);
    Task<LivroResponseDto> ObterPorIdAsync(int id);
}
