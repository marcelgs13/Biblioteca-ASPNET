using BibliotecaAPI.DTOs;
using BibliotecaAPI.Exceptions;
using BibliotecaAPI.Models;
using BibliotecaAPI.Repositories;

namespace BibliotecaAPI.Services;

public class AutorService(IAutorRepository autorRepository) : IAutorService
{
    public async Task<AutorResponseDto> CriarAsync(CriarAutorDto dto)
    {
        var autor = new Autor
        {
            Nome = dto.Nome,
            DataNascimento = dto.DataNascimento,
            Nacionalidade = dto.Nacionalidade
        };

        await autorRepository.AdicionarAsync(autor);
        return Mapear(autor);
    }

    public async Task<List<AutorResponseDto>> ListarAsync()
    {
        var autores = await autorRepository.ListarAsync();
        return autores.Select(Mapear).ToList();
    }

    public async Task<AutorResponseDto> ObterPorIdAsync(int id)
    {
        var autor = await autorRepository.ObterPorIdAsync(id)
            ?? throw new RecursoNaoEncontradoException("Autor não encontrado.");

        return Mapear(autor);
    }

    private static AutorResponseDto Mapear(Autor autor)
    {
        return new AutorResponseDto
        {
            Id = autor.Id,
            Nome = autor.Nome,
            DataNascimento = autor.DataNascimento,
            Nacionalidade = autor.Nacionalidade
        };
    }
}
