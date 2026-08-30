namespace BibliotecaAPI.Models;

public class Notificacao
{
    public int Id { get; set; }
    public int AlunoId { get; set; }
    public Aluno Aluno { get; set; } = null!;
    public string Mensagem { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public string Tipo { get; set; } = string.Empty;
}
