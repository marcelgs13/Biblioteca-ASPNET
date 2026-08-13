using BibliotecaAPI.DTOs;
using BibliotecaAPI.Exceptions;
using BibliotecaAPI.Models;
using BibliotecaAPI.Repositories;

namespace BibliotecaAPI.Services;

public class EmprestimoService(
    IEmprestimoRepository emprestimoRepository,
    IAlunoRepository alunoRepository,
    ILivroRepository livroRepository) : IEmprestimoService
{
    public async Task<EmprestimoResponseDto> CriarAsync(CriarEmprestimoDto dto)
    {
        _ = await alunoRepository.ObterPorIdAsync(dto.AlunoId)
            ?? throw new RecursoNaoEncontradoException("Aluno não encontrado.");

        var livro = await livroRepository.ObterPorIdAsync(dto.LivroId)
            ?? throw new RecursoNaoEncontradoException("Livro não encontrado.");

        if (await emprestimoRepository.ExisteAtivoAsync(dto.AlunoId, dto.LivroId))
        {
            throw new ConflitoNegocioException(
                "O aluno já possui um empréstimo ativo deste livro.");
        }

        if (livro.Quantidade <= 0)
        {
            throw new ConflitoNegocioException(
                "O livro não possui exemplares disponíveis.");
        }

        var agora = DateTime.UtcNow;
        var emprestimo = new Emprestimo
        {
            AlunoId = dto.AlunoId,
            LivroId = dto.LivroId,
            DataEmprestimo = agora,
            DataPrevistaDevolucao = agora.AddDays(7),
            Status = StatusEmprestimo.Ativo
        };

        livro.Quantidade--;
        await emprestimoRepository.AdicionarAsync(emprestimo);

        return Mapear(emprestimo);
    }

    public async Task<EmprestimoResponseDto> DevolverAsync(int id)
    {
        var emprestimo = await emprestimoRepository.ObterPorIdComLivroAsync(id)
            ?? throw new RecursoNaoEncontradoException("Empréstimo não encontrado.");

        if (emprestimo.Status == StatusEmprestimo.Devolvido)
        {
            throw new ConflitoNegocioException("O empréstimo já foi devolvido.");
        }

        emprestimo.DataDevolucao = DateTime.UtcNow;
        emprestimo.Status = StatusEmprestimo.Devolvido;
        emprestimo.Livro.Quantidade++;

        await emprestimoRepository.SalvarAlteracoesAsync();
        return Mapear(emprestimo);
    }

    private static EmprestimoResponseDto Mapear(Emprestimo emprestimo)
    {
        return new EmprestimoResponseDto
        {
            Id = emprestimo.Id,
            AlunoId = emprestimo.AlunoId,
            LivroId = emprestimo.LivroId,
            DataEmprestimo = emprestimo.DataEmprestimo,
            DataPrevistaDevolucao = emprestimo.DataPrevistaDevolucao,
            DataDevolucao = emprestimo.DataDevolucao,
            Status = emprestimo.Status
        };
    }
}
