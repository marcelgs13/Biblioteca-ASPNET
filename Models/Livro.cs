namespace BibliotecaAPI.Models
{
    public class Livro
    {
        public int Id { get; set; }
        public string ISBN { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int AnoPublicacao { get; set; }
        public string Editora { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public string Localizacao { get; set; } = string.Empty;
        public int AutorId { get; set; }
        public Autor Autor { get; set; } = null!;

        public List<Emprestimo> Emprestimos { get; set; } = new List<Emprestimo>();
        public List<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}
