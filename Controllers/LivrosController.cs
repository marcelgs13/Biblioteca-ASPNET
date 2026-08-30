using BibliotecaAPI.DTOs;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LivrosController(IBibliotecaService bibliotecaService) : ControllerBase
{
    [Authorize(Roles = "ADMIN,BIBLIOTECARIO")]
    [HttpPost]
    public async Task<ActionResult<LivroResponseDto>> Criar(CriarLivroDto dto)
    {
        var livro = await bibliotecaService.CriarLivroAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = livro.Id }, livro);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponseDto<LivroResponseDto>>> Listar([FromQuery] LivroQueryDto query)
    {
        return Ok(await bibliotecaService.ListarLivrosAsync(query));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LivroResponseDto>> ObterPorId(int id)
    {
        return Ok(await bibliotecaService.ObterLivroPorIdAsync(id));
    }

    [Authorize(Roles = "ADMIN,BIBLIOTECARIO")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<LivroResponseDto>> Atualizar(int id, AtualizarLivroDto dto)
    {
        return Ok(await bibliotecaService.AtualizarLivroAsync(id, dto));
    }

    [Authorize(Roles = "ADMIN,BIBLIOTECARIO")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        await bibliotecaService.ExcluirLivroAsync(id);
        return NoContent();
    }
}
