namespace BibliotecaAPI.Models
{
    public class Livro
    {
        public int Id { get; set; }
        public string ISBN { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public int AnoPublicacao { get; set; }
        public int Quantidade { get; set; }
        public int AutorId { get; set; }
        public Autor Autor { get; set; } = null!;

        public List<Emprestimo> Emprestimos { get; set; } = new List<Emprestimo>();
    }
}
