using System.Security.Claims;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservasController(IBibliotecaService bibliotecaService) : ControllerBase
{
    [Authorize(Roles = "ALUNO")]
    [HttpPost]
    public async Task<ActionResult<ReservaResponseDto>> Criar(CriarReservaDto dto)
    {
        var reserva = await bibliotecaService.CriarReservaAsync(ObterAlunoId(), dto);
        return StatusCode(StatusCodes.Status201Created, reserva);
    }

    [HttpGet]
    public async Task<ActionResult<List<ReservaResponseDto>>> Listar()
    {
        int? alunoId = User.IsInRole("ALUNO") ? ObterAlunoId() : null;
        return Ok(await bibliotecaService.ListarReservasAsync(alunoId));
    }

    [Authorize(Roles = "ADMIN,BIBLIOTECARIO")]
    [HttpPut("{id:int}/aprovar")]
    public async Task<ActionResult<EmprestimoResponseDto>> Aprovar(int id)
    {
        return Ok(await bibliotecaService.AprovarReservaAsync(id));
    }

    [Authorize(Roles = "ADMIN,BIBLIOTECARIO")]
    [HttpPut("{id:int}/rejeitar")]
    public async Task<ActionResult<ReservaResponseDto>> Rejeitar(int id)
    {
        return Ok(await bibliotecaService.RejeitarReservaAsync(id));
    }

    [Authorize(Roles = "ALUNO")]
    [HttpPut("{id:int}/cancelar")]
    public async Task<ActionResult<ReservaResponseDto>> Cancelar(int id)
    {
        return Ok(await bibliotecaService.CancelarReservaAsync(id, ObterAlunoId()));
    }

    private int ObterAlunoId() => int.Parse(User.FindFirstValue("alunoId")!);
}
