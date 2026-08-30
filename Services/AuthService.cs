using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Exceptions;
using BibliotecaAPI.Models;
using BibliotecaAPI.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BibliotecaAPI.Services;

public class AuthService(
    IBibliotecaRepository repository,
    IPasswordHasher<Usuario> passwordHasher,
    IOptions<JwtSettings> jwtOptions) : IAuthService
{
    public async Task<List<BibliotecarioResponseDto>> ListarBibliotecariosAsync()
    {
        var usuarios = await repository.ListarBibliotecariosAsync();
        return usuarios.Select(MapearBibliotecario).ToList();
    }

    public async Task<TotalUsuariosResponseDto> ObterTotalUsuariosAsync()
    {
        return new TotalUsuariosResponseDto
        {
            Total = await repository.ContarUsuariosAsync()
        };
    }

    public async Task<BibliotecarioResponseDto> CriarBibliotecarioAsync(CriarBibliotecarioDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await repository.ObterUsuarioPorEmailAsync(email) is not null)
        {
            throw new ConflitoNegocioException("Já existe um usuário cadastrado com este e-mail.");
        }

        var usuario = new Usuario
        {
            Nome = dto.Nome.Trim(),
            Email = email,
            Perfil = PerfilUsuario.BIBLIOTECARIO
        };
        usuario.SenhaHash = passwordHasher.HashPassword(usuario, dto.Senha);

        await repository.AdicionarUsuarioAsync(usuario);
        return MapearBibliotecario(usuario);
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var usuario = await repository.ObterUsuarioPorEmailAsync(email);

        if (usuario is null ||
            passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, dto.Senha) ==
            PasswordVerificationResult.Failed)
        {
            throw new CredenciaisInvalidasException("E-mail ou senha inválidos.");
        }

        var configuracao = jwtOptions.Value;
        var expiracao = DateTime.UtcNow.AddMinutes(configuracao.ExpirationMinutes);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Perfil.ToString())
        };

        if (usuario.AlunoId.HasValue)
        {
            claims.Add(new Claim("alunoId", usuario.AlunoId.Value.ToString()));
        }

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuracao.Key));
        var token = new JwtSecurityToken(
            issuer: configuracao.Issuer,
            audience: configuracao.Audience,
            claims: claims,
            expires: expiracao,
            signingCredentials: new SigningCredentials(chave, SecurityAlgorithms.HmacSha256));

        return new LoginResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiraEm = expiracao,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Perfil = usuario.Perfil.ToString(),
            AlunoId = usuario.AlunoId
        };
    }

    private static BibliotecarioResponseDto MapearBibliotecario(Usuario usuario)
    {
        return new BibliotecarioResponseDto
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email
        };
    }
}
