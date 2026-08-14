using BibliotecaAPI.DTOs;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlunosController(IAlunoService alunoService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AlunoResponseDto>> Criar(CriarAlunoDto dto)
    {
        var aluno = await alunoService.CriarAsync(dto);
        return StatusCode(StatusCodes.Status201Created, aluno);
    }
}
