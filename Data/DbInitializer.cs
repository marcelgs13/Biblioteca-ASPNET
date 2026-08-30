using BibliotecaAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Data;

public static class DbInitializer
{
    public static async Task InicializarAsync(IServiceProvider services, IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
        {
            return;
        }

        using var escopo = services.CreateScope();
        var context = escopo.ServiceProvider.GetRequiredService<BibliotecaDbContext>();
        var passwordHasher = escopo.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();

        await context.Database.MigrateAsync();

        await CriarUsuarioAsync(context, passwordHasher,
            "Administrador", "admin@ifpe.edu.br", "Admin@123", PerfilUsuario.ADMIN);
        await CriarUsuarioAsync(context, passwordHasher,
            "Bibliotecário", "bibliotecario@ifpe.edu.br", "Biblio@123", PerfilUsuario.BIBLIOTECARIO);

        const string emailAluno = "aluno@ifpe.edu.br";
        if (!await context.Usuarios.AnyAsync(usuario => usuario.Email == emailAluno))
        {
            var aluno = await context.Alunos.FirstOrDefaultAsync(item => item.Email == emailAluno);
            if (aluno is null)
            {
                aluno = new Aluno
                {
                    Nome = "José Miguel",
                    Matricula = "20232ewbj0030",
                    Email = emailAluno
                };
                context.Alunos.Add(aluno);
            }

            var usuario = new Usuario
            {
                Nome = aluno.Nome,
                Email = emailAluno,
                Perfil = PerfilUsuario.ALUNO,
                Aluno = aluno
            };
            usuario.SenhaHash = passwordHasher.HashPassword(usuario, "Aluno@123");
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();
        }
    }

    private static async Task CriarUsuarioAsync(
        BibliotecaDbContext context,
        IPasswordHasher<Usuario> passwordHasher,
        string nome,
        string email,
        string senha,
        PerfilUsuario perfil)
    {
        if (await context.Usuarios.AnyAsync(usuario => usuario.Email == email))
        {
            return;
        }

        var usuario = new Usuario
        {
            Nome = nome,
            Email = email,
            Perfil = perfil
        };
        usuario.SenhaHash = passwordHasher.HashPassword(usuario, senha);
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
    }
}
