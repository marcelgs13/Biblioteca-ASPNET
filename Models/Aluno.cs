namespace BibliotecaAPI.Models
{
    public class Aluno
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public List<Emprestimo> Emprestimos { get; set; } = new List<Emprestimo>();
            
    }
}
