namespace BibliotecaAPI.Models;

public class Reserva
{
    public int Id { get; set; }
    public int AlunoId { get; set; }
    public Aluno Aluno { get; set; } = null!;
    public int LivroId { get; set; }
    public Livro Livro { get; set; } = null!;
    public DateTime DataReserva { get; set; }
    public StatusReserva Status { get; set; }
}

public enum StatusReserva
{
    AguardandoDisponibilidade = 0,
    AguardandoAprovacao = 1,
    Aprovada = 2,
    Rejeitada = 3,
    Cancelada = 4
}
