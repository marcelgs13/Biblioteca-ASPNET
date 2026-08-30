using System.Security.Claims;
using BibliotecaAPI.Models;
using BibliotecaAPI.Repositories;

namespace BibliotecaAPI.Data;

public class AuditoriaMiddleware(RequestDelegate next, ILogger<AuditoriaMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, IBibliotecaRepository repository)
    {
        await next(context);

        if (!DeveRegistrar(context)) return;

        try
        {
            var usuarioIdTexto = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(usuarioIdTexto, out var usuarioId)) return;

            var rota = context.Request.Path.Value ?? string.Empty;
            await repository.AdicionarAuditoriaAsync(new Auditoria
            {
                UsuarioId = usuarioId,
                UsuarioNome = context.User.FindFirstValue(ClaimTypes.Name) ?? "Usuário não identificado",
                Perfil = context.User.FindFirstValue(ClaimTypes.Role) ?? "Perfil não identificado",
                Acao = DescreverAcao(context.Request.Method, rota),
                Detalhes = $"{context.Request.Method} {rota}",
                Data = DateTime.UtcNow
            });
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Não foi possível persistir o registro de auditoria.");
        }
    }

    private static bool DeveRegistrar(HttpContext context)
    {
        var metodo = context.Request.Method;
        var alteraDados = HttpMethods.IsPost(metodo) || HttpMethods.IsPut(metodo) || HttpMethods.IsDelete(metodo);
        return alteraDados &&
            context.User.Identity?.IsAuthenticated == true &&
            context.Response.StatusCode is >= 200 and < 300 &&
            !context.Request.Path.StartsWithSegments("/api/auth");
    }

    private static string DescreverAcao(string metodo, string rota)
    {
        var caminho = rota.ToLowerInvariant();

        if (HttpMethods.IsPost(metodo))
        {
            if (caminho == "/api/autores") return "Cadastrou um autor";
            if (caminho == "/api/livros") return "Cadastrou um livro";
            if (caminho == "/api/alunos") return "Cadastrou um aluno";
            if (caminho == "/api/emprestimos") return "Registrou um empréstimo";
            if (caminho == "/api/reservas") return "Solicitou um empréstimo";
            if (caminho == "/api/usuarios/bibliotecarios") return "Cadastrou um bibliotecário";
        }

        if (HttpMethods.IsPut(metodo))
        {
            if (caminho.Contains("/devolucao")) return "Registrou uma devolução";
            if (caminho.Contains("/reservas/") && caminho.EndsWith("/aprovar")) return "Aprovou uma solicitação";
            if (caminho.Contains("/reservas/") && caminho.EndsWith("/rejeitar")) return "Rejeitou uma solicitação";
            if (caminho.Contains("/reservas/") && caminho.EndsWith("/cancelar")) return "Cancelou uma solicitação";
            if (caminho.StartsWith("/api/autores/")) return "Atualizou um autor";
            if (caminho.StartsWith("/api/livros/")) return "Atualizou um livro";
        }

        if (HttpMethods.IsDelete(metodo))
        {
            if (caminho.StartsWith("/api/autores/")) return "Excluiu um autor";
            if (caminho.StartsWith("/api/livros/")) return "Excluiu um livro";
            if (caminho.StartsWith("/api/alunos/")) return "Excluiu um aluno";
        }

        return "Alterou dados do sistema";
    }
}
