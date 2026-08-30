using BibliotecaAPI.DTOs;
using BibliotecaAPI.Exceptions;
using BibliotecaAPI.Models;
using BibliotecaAPI.Repositories;
using Microsoft.AspNetCore.Identity;

namespace BibliotecaAPI.Services;

public class BibliotecaService(
    IBibliotecaRepository repository,
    IPasswordHasher<Usuario> passwordHasher) : IBibliotecaService
{
    private const decimal MultaPorDia = 2m;

    public async Task<AutorResponseDto> CriarAutorAsync(CriarAutorDto dto)
    {
        var autor = new Autor { Nome = dto.Nome.Trim(), DataNascimento = dto.DataNascimento, Nacionalidade = dto.Nacionalidade.Trim() };
        await repository.AdicionarAutorAsync(autor);
        return MapearAutor(autor);
    }

    public async Task<List<AutorResponseDto>> ListarAutoresAsync() =>
        (await repository.ListarAutoresAsync()).Select(MapearAutor).ToList();

    public async Task<AutorResponseDto> ObterAutorPorIdAsync(int id) => MapearAutor(await ObterAutorAsync(id));

    public async Task<AutorResponseDto> AtualizarAutorAsync(int id, AtualizarAutorDto dto)
    {
        var autor = await ObterAutorAsync(id);
        autor.Nome = dto.Nome.Trim();
        autor.DataNascimento = dto.DataNascimento;
        autor.Nacionalidade = dto.Nacionalidade.Trim();
        await repository.AtualizarAutorAsync(autor);
        return MapearAutor(autor);
    }

    public async Task ExcluirAutorAsync(int id)
    {
        var autor = await ObterAutorAsync(id);
        if (await repository.AutorPossuiLivrosAsync(id))
            throw new ConflitoNegocioException("Não é possível excluir o autor porque ele possui livros cadastrados.");
        await repository.RemoverAutorAsync(autor);
    }

    public async Task<LivroResponseDto> CriarLivroAsync(CriarLivroDto dto)
    {
        var isbn = dto.ISBN.Trim();
        if (await repository.ExisteISBNAsync(isbn))
            throw new ConflitoNegocioException("Já existe um livro cadastrado com este ISBN.");

        var autor = await ObterAutorAsync(dto.AutorId);
        var livro = new Livro
        {
            ISBN = isbn,
            Titulo = dto.Titulo.Trim(),
            Descricao = dto.Descricao.Trim(),
            AnoPublicacao = dto.AnoPublicacao,
            Editora = dto.Editora.Trim(),
            Categoria = dto.Categoria.Trim(),
            Quantidade = dto.Quantidade,
            Localizacao = dto.Localizacao.Trim(),
            AutorId = dto.AutorId
        };
        await repository.AdicionarLivroAsync(livro);
        return MapearLivro(livro, autor.Nome);
    }

    public async Task<PagedResponseDto<LivroResponseDto>> ListarLivrosAsync(LivroQueryDto query)
    {
        var (livros, total) = await repository.ListarLivrosAsync(query.Termo, query.Titulo, query.Autor, query.Page, query.PageSize);
        return new PagedResponseDto<LivroResponseDto>
        {
            Items = livros.Select(livro => MapearLivro(livro)).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = total,
            TotalPages = (int)Math.Ceiling(total / (double)query.PageSize)
        };
    }

    public async Task<LivroResponseDto> ObterLivroPorIdAsync(int id) => MapearLivro(await ObterLivroAsync(id));

    public async Task<LivroResponseDto> AtualizarLivroAsync(int id, AtualizarLivroDto dto)
    {
        var livro = await ObterLivroAsync(id);
        var quantidadeAnterior = livro.Quantidade;
        var isbn = dto.ISBN.Trim();
        if (!string.Equals(livro.ISBN, isbn, StringComparison.OrdinalIgnoreCase) && await repository.ExisteISBNAsync(isbn))
            throw new ConflitoNegocioException("Já existe um livro cadastrado com este ISBN.");

        var autor = await ObterAutorAsync(dto.AutorId);
        livro.ISBN = isbn;
        livro.Titulo = dto.Titulo.Trim();
        livro.Descricao = dto.Descricao.Trim();
        livro.AnoPublicacao = dto.AnoPublicacao;
        livro.Editora = dto.Editora.Trim();
        livro.Categoria = dto.Categoria.Trim();
        livro.Quantidade = dto.Quantidade;
        livro.Localizacao = dto.Localizacao.Trim();
        livro.AutorId = dto.AutorId;
        if (livro.Quantidade > quantidadeAnterior)
        {
            await PromoverSolicitacoesAsync(livro, DateTime.UtcNow);
        }
        await repository.AtualizarLivroAsync(livro);
        return MapearLivro(livro, autor.Nome);
    }

    public async Task ExcluirLivroAsync(int id)
    {
        var livro = await ObterLivroAsync(id);
        if (await repository.LivroPossuiVinculosAsync(id))
            throw new ConflitoNegocioException("Não é possível excluir o livro porque ele possui empréstimos ou reservas.");
        await repository.RemoverLivroAsync(livro);
    }

    public async Task<AlunoResponseDto> CriarAlunoAsync(CriarAlunoDto dto)
    {
        var matricula = dto.Matricula.Trim();
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await repository.ExisteMatriculaAsync(matricula))
            throw new ConflitoNegocioException("Já existe um aluno com esta matrícula.");
        if (await repository.ExisteEmailAsync(email) || await repository.ObterUsuarioPorEmailAsync(email) is not null)
            throw new ConflitoNegocioException("Já existe um aluno cadastrado com este e-mail.");

        var aluno = new Aluno { Nome = dto.Nome.Trim(), Matricula = matricula, Email = email };
        var usuario = new Usuario { Nome = aluno.Nome, Email = email, Perfil = PerfilUsuario.ALUNO, Aluno = aluno };
        usuario.SenhaHash = passwordHasher.HashPassword(usuario, dto.Senha);
        await repository.AdicionarAlunoComUsuarioAsync(aluno, usuario);
        return MapearAluno(aluno);
    }

    public async Task<List<AlunoResponseDto>> ListarAlunosAsync() =>
        (await repository.ListarAlunosAsync()).Select(MapearAluno).ToList();

    public async Task<AlunoResponseDto> ObterAlunoPorIdAsync(int id) => MapearAluno(await ObterAlunoAsync(id));

    public async Task ExcluirAlunoAsync(int id)
    {
        var aluno = await ObterAlunoAsync(id);
        if (await repository.AlunoPossuiEmprestimosAsync(id))
            throw new ConflitoNegocioException("Não é possível excluir o aluno porque ele possui empréstimos, reservas ou notificações.");
        await repository.RemoverAlunoAsync(aluno);
    }

    public async Task<EmprestimoResponseDto> CriarEmprestimoAsync(CriarEmprestimoDto dto)
    {
        await repository.AtualizarEmprestimosAtrasadosAsync(DateTime.UtcNow);
        _ = await ObterAlunoAsync(dto.AlunoId);
        var livro = await ObterLivroAsync(dto.LivroId);
        if (await repository.ExisteEmprestimoAtivoAsync(dto.AlunoId, dto.LivroId))
            throw new ConflitoNegocioException("O aluno já possui um empréstimo aberto deste livro.");

        var reservaDisponivel = await repository.ObterReservaAguardandoAprovacaoAsync(dto.LivroId, dto.AlunoId);
        var solicitacoesComVaga = await repository.ContarReservasAguardandoAprovacaoAsync(dto.LivroId);
        if (livro.Quantidade <= 0)
            throw new ConflitoNegocioException("O livro não possui exemplares disponíveis.");
        if (reservaDisponivel is null && livro.Quantidade <= solicitacoesComVaga)
            throw new ConflitoNegocioException("Os exemplares disponíveis estão vinculados a outras solicitações.");

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
        if (reservaDisponivel is not null)
        {
            reservaDisponivel.Status = StatusReserva.Aprovada;
            repository.AdicionarNotificacao(new Notificacao
            {
                AlunoId = reservaDisponivel.AlunoId,
                Mensagem = $"Sua solicitação para o livro '{livro.Titulo}' foi aprovada.",
                Data = agora,
                Tipo = "SolicitacaoAprovada"
            });
        }
        await repository.AdicionarEmprestimoAsync(emprestimo);
        return MapearEmprestimo(emprestimo, agora);
    }

    public async Task<List<EmprestimoResponseDto>> ListarEmprestimosAsync(int? alunoId = null)
    {
        var agora = DateTime.UtcNow;
        await repository.AtualizarEmprestimosAtrasadosAsync(agora);
        return (await repository.ListarEmprestimosAsync(alunoId)).Select(item => MapearEmprestimo(item, agora)).ToList();
    }

    public async Task<EmprestimoResponseDto> ObterEmprestimoPorIdAsync(int id, int? alunoId = null)
    {
        var agora = DateTime.UtcNow;
        await repository.AtualizarEmprestimosAtrasadosAsync(agora);
        var emprestimo = await repository.ObterEmprestimoPorIdAsync(id)
            ?? throw new RecursoNaoEncontradoException("Empréstimo não encontrado.");
        if (alunoId.HasValue && emprestimo.AlunoId != alunoId.Value)
            throw new RecursoNaoEncontradoException("Empréstimo não encontrado.");
        return MapearEmprestimo(emprestimo, agora);
    }

    public async Task<EmprestimoResponseDto> DevolverEmprestimoAsync(int id)
    {
        var emprestimo = await repository.ObterEmprestimoPorIdComLivroAsync(id)
            ?? throw new RecursoNaoEncontradoException("Empréstimo não encontrado.");
        if (emprestimo.Status == StatusEmprestimo.Devolvido)
            throw new ConflitoNegocioException("O empréstimo já foi devolvido.");

        var agora = DateTime.UtcNow;
        emprestimo.DataDevolucao = agora;
        emprestimo.Status = StatusEmprestimo.Devolvido;
        emprestimo.Livro.Quantidade++;
        await PromoverSolicitacoesAsync(emprestimo.Livro, agora);
        await repository.SalvarAlteracoesAsync();
        return MapearEmprestimo(emprestimo, agora);
    }

    public async Task<ReservaResponseDto> CriarReservaAsync(int alunoId, CriarReservaDto dto)
    {
        var aluno = await ObterAlunoAsync(alunoId);
        var livro = await ObterLivroAsync(dto.LivroId);
        if (await repository.ExisteReservaAtivaAsync(alunoId, dto.LivroId))
            throw new ConflitoNegocioException("O aluno já possui uma solicitação ativa deste livro.");
        if (await repository.ExisteEmprestimoAtivoAsync(alunoId, dto.LivroId))
            throw new ConflitoNegocioException("O aluno já possui um empréstimo aberto deste livro.");

        var aguardandoAprovacao = await repository.ContarReservasAguardandoAprovacaoAsync(livro.Id);
        var status = livro.Quantidade > aguardandoAprovacao
            ? StatusReserva.AguardandoAprovacao
            : StatusReserva.AguardandoDisponibilidade;

        var reserva = new Reserva
        {
            AlunoId = alunoId,
            LivroId = livro.Id,
            DataReserva = DateTime.UtcNow,
            Status = status
        };
        await repository.AdicionarReservaAsync(reserva);
        int? quantidadeAFrente = reserva.Status == StatusReserva.AguardandoDisponibilidade
            ? CalcularQuantidadeAFrente(reserva, await repository.ListarReservasAsync())
            : null;
        return new ReservaResponseDto
        {
            Id = reserva.Id,
            AlunoId = aluno.Id,
            AlunoNome = aluno.Nome,
            LivroId = livro.Id,
            LivroTitulo = livro.Titulo,
            DataReserva = reserva.DataReserva,
            Status = reserva.Status,
            QuantidadeAFrente = quantidadeAFrente
        };
    }

    public async Task<List<ReservaResponseDto>> ListarReservasAsync(int? alunoId = null)
    {
        var todas = await repository.ListarReservasAsync();
        return todas
            .Where(reserva => !alunoId.HasValue || reserva.AlunoId == alunoId.Value)
            .Select(reserva => MapearReserva(
                reserva,
                reserva.Status == StatusReserva.AguardandoDisponibilidade
                    ? CalcularQuantidadeAFrente(reserva, todas)
                    : null))
            .ToList();
    }

    public async Task<EmprestimoResponseDto> AprovarReservaAsync(int id)
    {
        var reserva = await ObterReservaAsync(id);
        if (reserva.Status != StatusReserva.AguardandoAprovacao)
            throw new ConflitoNegocioException("A solicitação não está aguardando aprovação.");
        if (await repository.ExisteEmprestimoAtivoAsync(reserva.AlunoId, reserva.LivroId))
            throw new ConflitoNegocioException("O aluno já possui um empréstimo aberto deste livro.");
        if (reserva.Livro.Quantidade <= 0)
            throw new ConflitoNegocioException("O livro não possui exemplares disponíveis para aprovar a solicitação.");

        var agora = DateTime.UtcNow;
        var emprestimo = new Emprestimo
        {
            AlunoId = reserva.AlunoId,
            LivroId = reserva.LivroId,
            DataEmprestimo = agora,
            DataPrevistaDevolucao = agora.AddDays(7),
            Status = StatusEmprestimo.Ativo
        };
        reserva.Status = StatusReserva.Aprovada;
        reserva.Livro.Quantidade--;
        repository.AdicionarNotificacao(new Notificacao
        {
            AlunoId = reserva.AlunoId,
            Mensagem = $"Sua solicitação para o livro '{reserva.Livro.Titulo}' foi aprovada.",
            Data = agora,
            Tipo = "SolicitacaoAprovada"
        });
        await repository.AdicionarEmprestimoAsync(emprestimo);
        return MapearEmprestimo(emprestimo, agora);
    }

    public async Task<ReservaResponseDto> RejeitarReservaAsync(int id)
    {
        var reserva = await ObterReservaAsync(id);
        ValidarSolicitacaoAberta(reserva);
        var liberouVaga = reserva.Status == StatusReserva.AguardandoAprovacao;
        reserva.Status = StatusReserva.Rejeitada;
        var agora = DateTime.UtcNow;
        repository.AdicionarNotificacao(new Notificacao
        {
            AlunoId = reserva.AlunoId,
            Mensagem = $"Sua solicitação para o livro '{reserva.Livro.Titulo}' foi rejeitada.",
            Data = agora,
            Tipo = "SolicitacaoRejeitada"
        });
        if (liberouVaga) await PromoverSolicitacoesAsync(reserva.Livro, agora, 1);
        await repository.SalvarAlteracoesAsync();
        return MapearReserva(reserva);
    }

    public async Task<ReservaResponseDto> CancelarReservaAsync(int id, int alunoId)
    {
        var reserva = await ObterReservaAsync(id);
        if (reserva.AlunoId != alunoId)
            throw new RecursoNaoEncontradoException("Solicitação não encontrada.");
        ValidarSolicitacaoAberta(reserva);
        var liberouVaga = reserva.Status == StatusReserva.AguardandoAprovacao;
        reserva.Status = StatusReserva.Cancelada;
        if (liberouVaga) await PromoverSolicitacoesAsync(reserva.Livro, DateTime.UtcNow, 1);
        await repository.SalvarAlteracoesAsync();
        return MapearReserva(reserva);
    }

    public async Task<List<NotificacaoResponseDto>> ListarNotificacoesAsync(int alunoId)
    {
        _ = await ObterAlunoAsync(alunoId);
        return (await repository.ListarNotificacoesAsync(alunoId)).Select(item => new NotificacaoResponseDto
        {
            Id = item.Id,
            Mensagem = item.Mensagem,
            Data = item.Data,
            Tipo = item.Tipo
        }).ToList();
    }

    public async Task<List<LivroMaisEmprestadoResponseDto>> ListarLivrosMaisEmprestadosAsync()
    {
        var emprestimos = await repository.ListarEmprestimosParaRelatorioAsync();
        return emprestimos
            .GroupBy(emprestimo => new
            {
                emprestimo.LivroId,
                emprestimo.Livro.Titulo,
                AutorNome = emprestimo.Livro.Autor.Nome
            })
            .Select(grupo => new LivroMaisEmprestadoResponseDto
            {
                LivroId = grupo.Key.LivroId,
                Titulo = grupo.Key.Titulo,
                AutorNome = grupo.Key.AutorNome,
                QuantidadeEmprestimos = grupo.Count()
            })
            .OrderByDescending(item => item.QuantidadeEmprestimos)
            .ThenBy(item => item.Titulo)
            .ToList();
    }

    public async Task<List<UsuarioInadimplenteResponseDto>> ListarUsuariosInadimplentesAsync()
    {
        var agora = DateTime.UtcNow;
        await repository.AtualizarEmprestimosAtrasadosAsync(agora);
        var emprestimos = await repository.ListarEmprestimosParaRelatorioAsync();

        return emprestimos
            .Where(emprestimo =>
                emprestimo.Status == StatusEmprestimo.Atrasado &&
                CalcularDiasAtraso(emprestimo, agora) > 0)
            .GroupBy(emprestimo => new
            {
                emprestimo.AlunoId,
                emprestimo.Aluno.Nome,
                emprestimo.Aluno.Matricula,
                emprestimo.Aluno.Email
            })
            .Select(grupo =>
            {
                var diasAtraso = grupo.Sum(emprestimo => CalcularDiasAtraso(emprestimo, agora));
                return new UsuarioInadimplenteResponseDto
                {
                    AlunoId = grupo.Key.AlunoId,
                    Nome = grupo.Key.Nome,
                    Matricula = grupo.Key.Matricula,
                    Email = grupo.Key.Email,
                    QuantidadeEmprestimosAtrasados = grupo.Count(),
                    DiasAtrasoTotal = diasAtraso,
                    MultaTotal = diasAtraso * MultaPorDia
                };
            })
            .OrderByDescending(item => item.MultaTotal)
            .ThenBy(item => item.Nome)
            .ToList();
    }

    public async Task<List<HistoricoEmprestimoResponseDto>> ListarHistoricoEmprestimosAsync(
        DateTime dataInicio,
        DateTime dataFim)
    {
        var agora = DateTime.UtcNow;
        await repository.AtualizarEmprestimosAtrasadosAsync(agora);
        var emprestimos = await repository.ListarEmprestimosParaRelatorioAsync(dataInicio, dataFim);

        return emprestimos.Select(emprestimo =>
        {
            var diasAtraso = CalcularDiasAtraso(emprestimo, agora);
            return new HistoricoEmprestimoResponseDto
            {
                Id = emprestimo.Id,
                AlunoId = emprestimo.AlunoId,
                AlunoNome = emprestimo.Aluno.Nome,
                Matricula = emprestimo.Aluno.Matricula,
                LivroId = emprestimo.LivroId,
                LivroTitulo = emprestimo.Livro.Titulo,
                DataEmprestimo = emprestimo.DataEmprestimo,
                DataPrevistaDevolucao = emprestimo.DataPrevistaDevolucao,
                DataDevolucao = emprestimo.DataDevolucao,
                Status = emprestimo.Status,
                DiasAtraso = diasAtraso,
                Multa = diasAtraso * MultaPorDia
            };
        }).ToList();
    }

    public async Task<PagedResponseDto<AuditoriaResponseDto>> ListarAuditoriasAsync(AuditoriaQueryDto query)
    {
        var (auditorias, total) = await repository.ListarAuditoriasAsync(query.Page, query.PageSize);
        return new PagedResponseDto<AuditoriaResponseDto>
        {
            Items = auditorias.Select(auditoria => new AuditoriaResponseDto
            {
                Id = auditoria.Id,
                UsuarioId = auditoria.UsuarioId,
                UsuarioNome = auditoria.UsuarioNome,
                Perfil = auditoria.Perfil,
                Acao = auditoria.Acao,
                Detalhes = auditoria.Detalhes,
                Data = DateTime.SpecifyKind(auditoria.Data, DateTimeKind.Utc)
            }).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = total,
            TotalPages = (int)Math.Ceiling(total / (double)query.PageSize)
        };
    }

    private async Task<Autor> ObterAutorAsync(int id) => await repository.ObterAutorPorIdAsync(id)
        ?? throw new RecursoNaoEncontradoException("Autor não encontrado.");
    private async Task<Livro> ObterLivroAsync(int id) => await repository.ObterLivroPorIdAsync(id)
        ?? throw new RecursoNaoEncontradoException("Livro não encontrado.");
    private async Task<Aluno> ObterAlunoAsync(int id) => await repository.ObterAlunoPorIdAsync(id)
        ?? throw new RecursoNaoEncontradoException("Aluno não encontrado.");
    private async Task<Reserva> ObterReservaAsync(int id) => await repository.ObterReservaPorIdAsync(id)
        ?? throw new RecursoNaoEncontradoException("Solicitação não encontrada.");

    private static void ValidarSolicitacaoAberta(Reserva reserva)
    {
        if (reserva.Status != StatusReserva.AguardandoDisponibilidade &&
            reserva.Status != StatusReserva.AguardandoAprovacao)
            throw new ConflitoNegocioException("A solicitação já foi finalizada.");
    }

    private async Task PromoverSolicitacoesAsync(Livro livro, DateTime agora, int vagasLiberadas = 0)
    {
        var aguardandoAprovacao = await repository.ContarReservasAguardandoAprovacaoAsync(livro.Id);
        var vagas = Math.Max(0, livro.Quantidade - aguardandoAprovacao + vagasLiberadas);
        if (vagas == 0) return;

        var proximas = await repository.ListarReservasAguardandoAsync(livro.Id, vagas);
        foreach (var reserva in proximas)
        {
            reserva.Status = StatusReserva.AguardandoAprovacao;
            repository.AdicionarNotificacao(new Notificacao
            {
                AlunoId = reserva.AlunoId,
                Mensagem = $"O livro '{livro.Titulo}' está disponível e sua solicitação aguarda aprovação.",
                Data = agora,
                Tipo = "SolicitacaoAguardandoAprovacao"
            });
        }
    }

    private static AutorResponseDto MapearAutor(Autor autor) => new()
    {
        Id = autor.Id, Nome = autor.Nome, DataNascimento = autor.DataNascimento, Nacionalidade = autor.Nacionalidade
    };

    private static LivroResponseDto MapearLivro(Livro livro, string? autorNome = null) => new()
    {
        Id = livro.Id, ISBN = livro.ISBN, Titulo = livro.Titulo, Descricao = livro.Descricao,
        AnoPublicacao = livro.AnoPublicacao, Editora = livro.Editora, Categoria = livro.Categoria,
        Quantidade = livro.Quantidade, Localizacao = livro.Localizacao, AutorId = livro.AutorId,
        AutorNome = autorNome ?? livro.Autor.Nome
    };

    private static AlunoResponseDto MapearAluno(Aluno aluno) => new()
    {
        Id = aluno.Id, Nome = aluno.Nome, Matricula = aluno.Matricula, Email = aluno.Email
    };

    private static EmprestimoResponseDto MapearEmprestimo(Emprestimo emprestimo, DateTime agora)
    {
        var dias = CalcularDiasAtraso(emprestimo, agora);
        return new EmprestimoResponseDto
        {
            Id = emprestimo.Id, AlunoId = emprestimo.AlunoId, LivroId = emprestimo.LivroId,
            DataEmprestimo = emprestimo.DataEmprestimo,
            DataPrevistaDevolucao = emprestimo.DataPrevistaDevolucao,
            DataDevolucao = emprestimo.DataDevolucao, Status = emprestimo.Status,
            DiasAtraso = dias, Multa = dias * MultaPorDia
        };
    }

    private static int CalcularDiasAtraso(Emprestimo emprestimo, DateTime agora)
    {
        var referencia = emprestimo.DataDevolucao ?? agora;
        return referencia.Date > emprestimo.DataPrevistaDevolucao.Date
            ? (referencia.Date - emprestimo.DataPrevistaDevolucao.Date).Days
            : 0;
    }

    private static int CalcularQuantidadeAFrente(Reserva reserva, IEnumerable<Reserva> reservas)
    {
        return reservas.Count(item =>
            item.LivroId == reserva.LivroId &&
            (item.Status == StatusReserva.AguardandoDisponibilidade ||
             item.Status == StatusReserva.AguardandoAprovacao) &&
            (item.DataReserva < reserva.DataReserva ||
             (item.DataReserva == reserva.DataReserva && item.Id < reserva.Id)));
    }

    private static ReservaResponseDto MapearReserva(Reserva reserva, int? quantidadeAFrente = null) => new()
    {
        Id = reserva.Id, AlunoId = reserva.AlunoId, AlunoNome = reserva.Aluno.Nome,
        LivroId = reserva.LivroId, LivroTitulo = reserva.Livro.Titulo,
        DataReserva = reserva.DataReserva, Status = reserva.Status,
        QuantidadeAFrente = quantidadeAFrente
    };
}
