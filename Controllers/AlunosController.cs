using BibliotecaAPI.DTOs;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN,BIBLIOTECARIO")]
public class AlunosController(IBibliotecaService bibliotecaService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AlunoResponseDto>> Criar(CriarAlunoDto dto)
    {
        var aluno = await bibliotecaService.CriarAlunoAsync(dto);
        return StatusCode(StatusCodes.Status201Created, aluno);
    }

    [HttpGet]
    public async Task<ActionResult<List<AlunoResponseDto>>> Listar()
    {
        return Ok(await bibliotecaService.ListarAlunosAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AlunoResponseDto>> ObterPorId(int id)
    {
        return Ok(await bibliotecaService.ObterAlunoPorIdAsync(id));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        await bibliotecaService.ExcluirAlunoAsync(id);
        return NoContent();
    }
}
