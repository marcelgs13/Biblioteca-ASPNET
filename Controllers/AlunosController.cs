using BibliotecaAPI.DTOs;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlunosController(IBibliotecaService bibliotecaService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AlunoResponseDto>> Criar(CriarAlunoDto dto)
    {
        var aluno = await bibliotecaService.CriarAlunoAsync(dto);
        return StatusCode(StatusCodes.Status201Created, aluno);
    }
}
