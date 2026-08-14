using BibliotecaAPI.DTOs;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LivrosController(IBibliotecaService bibliotecaService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<LivroResponseDto>> Criar(CriarLivroDto dto)
    {
        var livro = await bibliotecaService.CriarLivroAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = livro.Id }, livro);
    }

    [HttpGet]
    public async Task<ActionResult<List<LivroResponseDto>>> Listar(
        [FromQuery] string? titulo,
        [FromQuery] string? autor)
    {
        return Ok(await bibliotecaService.ListarLivrosAsync(titulo, autor));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LivroResponseDto>> ObterPorId(int id)
    {
        return Ok(await bibliotecaService.ObterLivroPorIdAsync(id));
    }
}
