using BibliotecaAPI.Models;

namespace BibliotecaAPI.DTOs;

public class ReservaResponseDto
{
    public int Id { get; set; }
    public int AlunoId { get; set; }
    public string AlunoNome { get; set; } = string.Empty;
    public int LivroId { get; set; }
    public string LivroTitulo { get; set; } = string.Empty;
    public DateTime DataReserva { get; set; }
    public StatusReserva Status { get; set; }
    public int? QuantidadeAFrente { get; set; }
}
