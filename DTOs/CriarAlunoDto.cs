using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs;

public class CriarAlunoDto
{
    [Required]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public string Matricula { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
