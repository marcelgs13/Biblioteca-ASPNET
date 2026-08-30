using BibliotecaAPI.DTOs;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmprestimosController(IBibliotecaService bibliotecaService) : ControllerBase
{
    [Authorize(Roles = "ADMIN,BIBLIOTECARIO")]
    [HttpPost]
    public async Task<ActionResult<EmprestimoResponseDto>> Criar(CriarEmprestimoDto dto)
    {
        var emprestimo = await bibliotecaService.CriarEmprestimoAsync(dto);
        return StatusCode(StatusCodes.Status201Created, emprestimo);
    }

    [HttpGet]
    public async Task<ActionResult<List<EmprestimoResponseDto>>> Listar()
    {
        return Ok(await bibliotecaService.ListarEmprestimosAsync(ObterFiltroAluno()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmprestimoResponseDto>> ObterPorId(int id)
    {
        return Ok(await bibliotecaService.ObterEmprestimoPorIdAsync(id, ObterFiltroAluno()));
    }

    [Authorize(Roles = "ADMIN,BIBLIOTECARIO")]
    [HttpPut("{id:int}/devolucao")]
    public async Task<ActionResult<EmprestimoResponseDto>> Devolver(int id)
    {
        return Ok(await bibliotecaService.DevolverEmprestimoAsync(id));
    }

    private int? ObterFiltroAluno()
    {
        if (!User.IsInRole("ALUNO")) return null;
        return int.Parse(User.FindFirstValue("alunoId")!);
    }
}
