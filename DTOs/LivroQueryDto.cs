using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs;

public class LivroQueryDto
{
    public string? Termo { get; set; }
    public string? Titulo { get; set; }
    public string? Autor { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}
