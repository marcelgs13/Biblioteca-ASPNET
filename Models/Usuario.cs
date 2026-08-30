namespace BibliotecaAPI.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public PerfilUsuario Perfil { get; set; }
    public int? AlunoId { get; set; }
    public Aluno? Aluno { get; set; }
}

public enum PerfilUsuario
{
    ADMIN = 0,
    BIBLIOTECARIO = 1,
    ALUNO = 2
}
