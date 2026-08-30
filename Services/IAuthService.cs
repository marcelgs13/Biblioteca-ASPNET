using BibliotecaAPI.DTOs;

namespace BibliotecaAPI.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
    Task<List<BibliotecarioResponseDto>> ListarBibliotecariosAsync();
    Task<BibliotecarioResponseDto> CriarBibliotecarioAsync(CriarBibliotecarioDto dto);
    Task<TotalUsuariosResponseDto> ObterTotalUsuariosAsync();
}
