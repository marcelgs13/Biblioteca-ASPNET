using BibliotecaAPI.DTOs;
using BibliotecaAPI.Exceptions;
using BibliotecaAPI.Models;
using BibliotecaAPI.Repositories;

namespace BibliotecaAPI.Services;

public class LivroService(
    ILivroRepository livroRepository,
    IAutorRepository autorRepository) : ILivroService
{
    public async Task<LivroResponseDto> CriarAsync(CriarLivroDto dto)
    {
        var autor = await autorRepository.ObterPorIdAsync(dto.AutorId)
            ?? throw new RecursoNaoEncontradoException("Autor não encontrado.");

        var livro = new Livro
        {
            ISBN = dto.ISBN,
            Titulo = dto.Titulo,
            AnoPublicacao = dto.AnoPublicacao,
            Quantidade = dto.Quantidade,
            AutorId = dto.AutorId
        };

        await livroRepository.AdicionarAsync(livro);
        return new LivroResponseDto
        {
            Id = livro.Id,
            ISBN = livro.ISBN,
            Titulo = livro.Titulo,
            AnoPublicacao = livro.AnoPublicacao,
            Quantidade = livro.Quantidade,
            AutorId = livro.AutorId,
            AutorNome = autor.Nome
        };
    }

    public async Task<List<LivroResponseDto>> ListarAsync(string? titulo, string? autor)
    {
        var livros = await livroRepository.ListarAsync(titulo, autor);
        return livros.Select(Mapear).ToList();
    }

    public async Task<LivroResponseDto> ObterPorIdAsync(int id)
    {
        var livro = await livroRepository.ObterPorIdAsync(id)
            ?? throw new RecursoNaoEncontradoException("Livro não encontrado.");

        return Mapear(livro);
    }

    private static LivroResponseDto Mapear(Livro livro)
    {
        return new LivroResponseDto
        {
            Id = livro.Id,
            ISBN = livro.ISBN,
            Titulo = livro.Titulo,
            AnoPublicacao = livro.AnoPublicacao,
            Quantidade = livro.Quantidade,
            AutorId = livro.AutorId,
            AutorNome = livro.Autor.Nome
        };
    }
}
