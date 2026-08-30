namespace BibliotecaAPI.DTOs;

public class NotificacaoResponseDto
{
    public int Id { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public string Tipo { get; set; } = string.Empty;
}
