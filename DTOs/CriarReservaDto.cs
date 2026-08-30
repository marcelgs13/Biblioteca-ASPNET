using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs;

public class CriarReservaDto
{
    [Range(1, int.MaxValue)]
    public int LivroId { get; set; }
}
