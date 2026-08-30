using BibliotecaAPI.Models;

namespace BibliotecaAPI.DTOs;

public class HistoricoEmprestimoResponseDto
{
    public int Id { get; set; }
    public int AlunoId { get; set; }
    public string AlunoNome { get; set; } = string.Empty;
    public string Matricula { get; set; } = string.Empty;
    public int LivroId { get; set; }
    public string LivroTitulo { get; set; } = string.Empty;
    public DateTime DataEmprestimo { get; set; }
    public DateTime DataPrevistaDevolucao { get; set; }
    public DateTime? DataDevolucao { get; set; }
    public StatusEmprestimo Status { get; set; }
    public int DiasAtraso { get; set; }
    public decimal Multa { get; set; }
}
