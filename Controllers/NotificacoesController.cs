using System.Security.Claims;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ALUNO")]
public class NotificacoesController(IBibliotecaService bibliotecaService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<NotificacaoResponseDto>>> Listar()
    {
        var alunoId = int.Parse(User.FindFirstValue("alunoId")!);
        return Ok(await bibliotecaService.ListarNotificacoesAsync(alunoId));
    }
}
