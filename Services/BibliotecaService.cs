using BibliotecaAPI.DTOs;
using BibliotecaAPI.Exceptions;
using BibliotecaAPI.Models;
using BibliotecaAPI.Repositories;

namespace BibliotecaAPI.Services;

public class BibliotecaService(IBibliotecaRepository repository) : IBibliotecaService
{
    public async Task<AutorResponseDto> CriarAutorAsync(CriarAutorDto dto)
    {
        var autor = new Autor
        {
            Nome = dto.Nome,
            DataNascimento = dto.DataNascimento,
            Nacionalidade = dto.Nacionalidade
        };

        await repository.AdicionarAutorAsync(autor);
        return MapearAutor(autor);
    }

    public async Task<List<AutorResponseDto>> ListarAutoresAsync()
    {
        var autores = await repository.ListarAutoresAsync();
        return autores.Select(MapearAutor).ToList();
    }

    public async Task<AutorResponseDto> ObterAutorPorIdAsync(int id)
    {
        var autor = await repository.ObterAutorPorIdAsync(id)
            ?? throw new RecursoNaoEncontradoException("Autor não encontrado.");

        return MapearAutor(autor);
    }

    public async Task<AutorResponseDto> AtualizarAutorAsync(int id, AtualizarAutorDto dto)
    {
        var autor = await repository.ObterAutorPorIdAsync(id)
            ?? throw new RecursoNaoEncontradoException("Autor não encontrado.");

        autor.Nome = dto.Nome;
        autor.DataNascimento = dto.DataNascimento;
        autor.Nacionalidade = dto.Nacionalidade;

        await repository.AtualizarAutorAsync(autor);
        return MapearAutor(autor);
    }

    public async Task ExcluirAutorAsync(int id)
    {
        var autor = await repository.ObterAutorPorIdAsync(id)
            ?? throw new RecursoNaoEncontradoException("Autor não encontrado.");

        if (await repository.AutorPossuiLivrosAsync(id))
        {
            throw new ConflitoNegocioException(
                "Não é possível excluir o autor porque ele possui livros cadastrados.");
        }

        await repository.RemoverAutorAsync(autor);
    }

    public async Task<LivroResponseDto> CriarLivroAsync(CriarLivroDto dto)
    {
        var isbn = dto.ISBN.Trim();

        if (await repository.ExisteISBNAsync(isbn))
        {
            throw new ConflitoNegocioException("Já existe um livro cadastrado com este ISBN.");
        }

        var autor = await repository.ObterAutorPorIdAsync(dto.AutorId)
            ?? throw new RecursoNaoEncontradoException("Autor não encontrado.");

        var livro = new Livro
        {
            ISBN = isbn,
            Titulo = dto.Titulo,
            AnoPublicacao = dto.AnoPublicacao,
            Quantidade = dto.Quantidade,
            AutorId = dto.AutorId
        };

        await repository.AdicionarLivroAsync(livro);

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

    public async Task<List<LivroResponseDto>> ListarLivrosAsync(string? titulo, string? autor)
    {
        var livros = await repository.ListarLivrosAsync(titulo, autor);
        return livros.Select(livro => MapearLivro(livro)).ToList();
    }

    public async Task<LivroResponseDto> ObterLivroPorIdAsync(int id)
    {
        var livro = await repository.ObterLivroPorIdAsync(id)
            ?? throw new RecursoNaoEncontradoException("Livro não encontrado.");

        return MapearLivro(livro);
    }

    public async Task<LivroResponseDto> AtualizarLivroAsync(int id, AtualizarLivroDto dto)
    {
        var livro = await repository.ObterLivroPorIdAsync(id)
            ?? throw new RecursoNaoEncontradoException("Livro não encontrado.");

        var autor = await repository.ObterAutorPorIdAsync(dto.AutorId)
            ?? throw new RecursoNaoEncontradoException("Autor não encontrado.");

        livro.ISBN = dto.ISBN;
        livro.Titulo = dto.Titulo;
        livro.AnoPublicacao = dto.AnoPublicacao;
        livro.Quantidade = dto.Quantidade;
        livro.AutorId = dto.AutorId;

        await repository.AtualizarLivroAsync(livro);
        return MapearLivro(livro, autor.Nome);
    }

    public async Task<AlunoResponseDto> CriarAlunoAsync(CriarAlunoDto dto)
    {
        if (await repository.ExisteMatriculaAsync(dto.Matricula))
        {
            throw new ConflitoNegocioException("Já existe um aluno com esta matrícula.");
        }

        var email = dto.Email.Trim().ToLowerInvariant();

        if (await repository.ExisteEmailAsync(email))
        {
            throw new ConflitoNegocioException("Já existe um aluno cadastrado com este e-mail.");
        }

        var aluno = new Aluno
        {
            Nome = dto.Nome,
            Matricula = dto.Matricula,
            Email = email
        };

        await repository.AdicionarAlunoAsync(aluno);

        return MapearAluno(aluno);
    }

    public async Task<List<AlunoResponseDto>> ListarAlunosAsync()
    {
        var alunos = await repository.ListarAlunosAsync();
        return alunos.Select(MapearAluno).ToList();
    }

    public async Task<AlunoResponseDto> ObterAlunoPorIdAsync(int id)
    {
        var aluno = await repository.ObterAlunoPorIdAsync(id)
            ?? throw new RecursoNaoEncontradoException("Aluno não encontrado.");

        return MapearAluno(aluno);
    }

    public async Task ExcluirAlunoAsync(int id)
    {
        var aluno = await repository.ObterAlunoPorIdAsync(id)
            ?? throw new RecursoNaoEncontradoException("Aluno não encontrado.");

        if (await repository.AlunoPossuiEmprestimosAsync(id))
        {
            throw new ConflitoNegocioException(
                "Não é possível excluir o aluno porque ele possui empréstimos cadastrados.");
        }

        await repository.RemoverAlunoAsync(aluno);
    }

    public async Task<EmprestimoResponseDto> CriarEmprestimoAsync(CriarEmprestimoDto dto)
    {
        _ = await repository.ObterAlunoPorIdAsync(dto.AlunoId)
            ?? throw new RecursoNaoEncontradoException("Aluno não encontrado.");

        var livro = await repository.ObterLivroPorIdAsync(dto.LivroId)
            ?? throw new RecursoNaoEncontradoException("Livro não encontrado.");

        if (await repository.ExisteEmprestimoAtivoAsync(dto.AlunoId, dto.LivroId))
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
        await repository.AdicionarEmprestimoAsync(emprestimo);

        return MapearEmprestimo(emprestimo);
    }

    public async Task<List<EmprestimoResponseDto>> ListarEmprestimosAsync()
    {
        var emprestimos = await repository.ListarEmprestimosAsync();
        return emprestimos.Select(MapearEmprestimo).ToList();
    }

    public async Task<EmprestimoResponseDto> ObterEmprestimoPorIdAsync(int id)
    {
        var emprestimo = await repository.ObterEmprestimoPorIdAsync(id)
            ?? throw new RecursoNaoEncontradoException("Empréstimo não encontrado.");

        return MapearEmprestimo(emprestimo);
    }

    public async Task<EmprestimoResponseDto> DevolverEmprestimoAsync(int id)
    {
        var emprestimo = await repository.ObterEmprestimoPorIdComLivroAsync(id)
            ?? throw new RecursoNaoEncontradoException("Empréstimo não encontrado.");

        if (emprestimo.Status == StatusEmprestimo.Devolvido)
        {
            throw new ConflitoNegocioException("O empréstimo já foi devolvido.");
        }

        emprestimo.DataDevolucao = DateTime.UtcNow;
        emprestimo.Status = StatusEmprestimo.Devolvido;
        emprestimo.Livro.Quantidade++;

        await repository.SalvarAlteracoesAsync();
        return MapearEmprestimo(emprestimo);
    }

    private static AutorResponseDto MapearAutor(Autor autor)
    {
        return new AutorResponseDto
        {
            Id = autor.Id,
            Nome = autor.Nome,
            DataNascimento = autor.DataNascimento,
            Nacionalidade = autor.Nacionalidade
        };
    }

    private static LivroResponseDto MapearLivro(Livro livro, string? autorNome = null)
    {
        return new LivroResponseDto
        {
            Id = livro.Id,
            ISBN = livro.ISBN,
            Titulo = livro.Titulo,
            AnoPublicacao = livro.AnoPublicacao,
            Quantidade = livro.Quantidade,
            AutorId = livro.AutorId,
            AutorNome = autorNome ?? livro.Autor.Nome
        };
    }

    private static AlunoResponseDto MapearAluno(Aluno aluno)
    {
        return new AlunoResponseDto
        {
            Id = aluno.Id,
            Nome = aluno.Nome,
            Matricula = aluno.Matricula,
            Email = aluno.Email
        };
    }

    private static EmprestimoResponseDto MapearEmprestimo(Emprestimo emprestimo)
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
