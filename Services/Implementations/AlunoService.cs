using BibliotecaAPI.DTOs;
using BibliotecaAPI.Exceptions;
using BibliotecaAPI.Models;
using BibliotecaAPI.Repositories;

namespace BibliotecaAPI.Services;

public class AlunoService(IAlunoRepository alunoRepository) : IAlunoService
{
    public async Task<AlunoResponseDto> CriarAsync(CriarAlunoDto dto)
    {
        if (await alunoRepository.ExisteMatriculaAsync(dto.Matricula))
        {
            throw new ConflitoNegocioException("Já existe um aluno com esta matrícula.");
        }

        var aluno = new Aluno
        {
            Nome = dto.Nome,
            Matricula = dto.Matricula,
            Email = dto.Email
        };

        await alunoRepository.AdicionarAsync(aluno);

        return new AlunoResponseDto
        {
            Id = aluno.Id,
            Nome = aluno.Nome,
            Matricula = aluno.Matricula,
            Email = aluno.Email
        };
    }
}
