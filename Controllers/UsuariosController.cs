using BibliotecaAPI.DTOs;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = "ADMIN")]
public class UsuariosController(IAuthService authService) : ControllerBase
{
    [HttpGet("total")]
    public async Task<ActionResult<TotalUsuariosResponseDto>> ObterTotal()
    {
        return Ok(await authService.ObterTotalUsuariosAsync());
    }

    [HttpGet("bibliotecarios")]
    public async Task<ActionResult<List<BibliotecarioResponseDto>>> ListarBibliotecarios()
    {
        return Ok(await authService.ListarBibliotecariosAsync());
    }

    [HttpPost("bibliotecarios")]
    public async Task<ActionResult<BibliotecarioResponseDto>> CriarBibliotecario(CriarBibliotecarioDto dto)
    {
        var bibliotecario = await authService.CriarBibliotecarioAsync(dto);
        return Created($"/api/usuarios/bibliotecarios/{bibliotecario.Id}", bibliotecario);
    }
}
