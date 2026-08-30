using BibliotecaAPI.DTOs;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN")]
public class AuditoriaController(IBibliotecaService bibliotecaService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponseDto<AuditoriaResponseDto>>> Listar(
        [FromQuery] AuditoriaQueryDto query)
    {
        return Ok(await bibliotecaService.ListarAuditoriasAsync(query));
    }
}
