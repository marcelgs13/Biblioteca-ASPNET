using BibliotecaAPI.DTOs;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmprestimosController(IEmprestimoService emprestimoService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<EmprestimoResponseDto>> Criar(CriarEmprestimoDto dto)
    {
        var emprestimo = await emprestimoService.CriarAsync(dto);
        return StatusCode(StatusCodes.Status201Created, emprestimo);
    }

    [HttpPut("{id:int}/devolucao")]
    public async Task<ActionResult<EmprestimoResponseDto>> Devolver(int id)
    {
        return Ok(await emprestimoService.DevolverAsync(id));
    }
}
