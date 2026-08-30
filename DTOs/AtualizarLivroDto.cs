using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs;

public class AtualizarLivroDto
{
    [Required]
    public string ISBN { get; set; } = string.Empty;

    [Required]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    public string Descricao { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int AnoPublicacao { get; set; }

    [Required]
    public string Editora { get; set; } = string.Empty;

    [Required]
    public string Categoria { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Quantidade { get; set; }

    [Required]
    public string Localizacao { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int AutorId { get; set; }
}
