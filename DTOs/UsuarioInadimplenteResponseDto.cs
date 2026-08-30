namespace BibliotecaAPI.DTOs;

public class UsuarioInadimplenteResponseDto
{
    public int AlunoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Matricula { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int QuantidadeEmprestimosAtrasados { get; set; }
    public int DiasAtrasoTotal { get; set; }
    public decimal MultaTotal { get; set; }
}
