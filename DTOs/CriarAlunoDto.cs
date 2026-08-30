using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs;

public class CriarAlunoDto
{
    private string _email = string.Empty;

    [Required]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public string Matricula { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email
    {
        get => _email;
        set => _email = value?.Trim() ?? string.Empty;
    }

    [Required]
    [MinLength(8)]
    public string Senha { get; set; } = string.Empty;
}
