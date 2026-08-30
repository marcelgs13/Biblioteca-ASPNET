namespace BibliotecaAPI.Models;

public class Auditoria
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public string Perfil { get; set; } = string.Empty;
    public string Acao { get; set; } = string.Empty;
    public string Detalhes { get; set; } = string.Empty;
    public DateTime Data { get; set; }
}
