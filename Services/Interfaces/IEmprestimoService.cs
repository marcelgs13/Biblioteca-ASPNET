using BibliotecaAPI.DTOs;

namespace BibliotecaAPI.Services;

public interface IEmprestimoService
{
    Task<EmprestimoResponseDto> CriarAsync(CriarEmprestimoDto dto);
    Task<EmprestimoResponseDto> DevolverAsync(int id);
}
