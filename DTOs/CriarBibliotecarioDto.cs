using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs;

public class CriarBibliotecarioDto
{
    [Required]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Senha { get; set; } = string.Empty;
}
