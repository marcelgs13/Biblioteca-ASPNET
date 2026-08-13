using BibliotecaAPI.DTOs;

namespace BibliotecaAPI.Services;

public interface IAutorService
{
    Task<AutorResponseDto> CriarAsync(CriarAutorDto dto);
    Task<List<AutorResponseDto>> ListarAsync();
    Task<AutorResponseDto> ObterPorIdAsync(int id);
}
