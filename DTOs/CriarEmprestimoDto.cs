using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs;

public class CriarEmprestimoDto
{
    [Range(1, int.MaxValue)]
    public int AlunoId { get; set; }

    [Range(1, int.MaxValue)]
    public int LivroId { get; set; }
}
