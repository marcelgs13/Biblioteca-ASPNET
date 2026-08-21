using BibliotecaAPI.DTOs;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AutoresController(IBibliotecaService bibliotecaService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AutorResponseDto>> Criar(CriarAutorDto dto)
    {
        var autor = await bibliotecaService.CriarAutorAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = autor.Id }, autor);
    }

    [HttpGet]
    public async Task<ActionResult<List<AutorResponseDto>>> Listar()
    {
        return Ok(await bibliotecaService.ListarAutoresAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AutorResponseDto>> ObterPorId(int id)
    {
        return Ok(await bibliotecaService.ObterAutorPorIdAsync(id));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AutorResponseDto>> Atualizar(int id, AtualizarAutorDto dto)
    {
        return Ok(await bibliotecaService.AtualizarAutorAsync(id, dto));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        await bibliotecaService.ExcluirAutorAsync(id);
        return NoContent();
    }
}
