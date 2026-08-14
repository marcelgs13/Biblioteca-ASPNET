using BibliotecaAPI.DTOs;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AutoresController(IAutorService autorService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AutorResponseDto>> Criar(CriarAutorDto dto)
    {
        var autor = await autorService.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = autor.Id }, autor);
    }

    [HttpGet]
    public async Task<ActionResult<List<AutorResponseDto>>> Listar()
    {
        return Ok(await autorService.ListarAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AutorResponseDto>> ObterPorId(int id)
    {
        return Ok(await autorService.ObterPorIdAsync(id));
    }
}
