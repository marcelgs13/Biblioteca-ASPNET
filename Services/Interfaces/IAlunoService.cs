using BibliotecaAPI.DTOs;

namespace BibliotecaAPI.Services;

public interface IAlunoService
{
    Task<AlunoResponseDto> CriarAsync(CriarAlunoDto dto);
}
