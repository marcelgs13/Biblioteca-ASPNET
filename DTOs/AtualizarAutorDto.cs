using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs;

public class AtualizarAutorDto
{
    [Required]
    public string Nome { get; set; } = string.Empty;

    public DateTime DataNascimento { get; set; }

    [Required]
    public string Nacionalidade { get; set; } = string.Empty;
}
