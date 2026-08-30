using BibliotecaAPI.DTOs;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN")]
public class RelatoriosController(IBibliotecaService bibliotecaService) : ControllerBase
{
    [HttpGet("livros-mais-emprestados")]
    public async Task<ActionResult<List<LivroMaisEmprestadoResponseDto>>> ListarLivrosMaisEmprestados()
    {
        return Ok(await bibliotecaService.ListarLivrosMaisEmprestadosAsync());
    }

    [HttpGet("usuarios-inadimplentes")]
    public async Task<ActionResult<List<UsuarioInadimplenteResponseDto>>> ListarUsuariosInadimplentes()
    {
        return Ok(await bibliotecaService.ListarUsuariosInadimplentesAsync());
    }

    [HttpGet("historico")]
    public async Task<ActionResult<List<HistoricoEmprestimoResponseDto>>> ListarHistorico(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim)
    {
        if (dataInicio == default || dataFim == default)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Período inválido",
                detail: "Informe dataInicio e dataFim.");
        }

        if (dataInicio.Date > dataFim.Date)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Período inválido",
                detail: "A data inicial não pode ser posterior à data final.");
        }

        return Ok(await bibliotecaService.ListarHistoricoEmprestimosAsync(dataInicio, dataFim));
    }
}
