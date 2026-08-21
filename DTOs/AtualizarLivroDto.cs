using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs;

public class AtualizarLivroDto
{
    [Required]
    public string ISBN { get; set; } = string.Empty;

    [Required]
    public string Titulo { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int AnoPublicacao { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantidade { get; set; }

    [Range(1, int.MaxValue)]
    public int AutorId { get; set; }
}
