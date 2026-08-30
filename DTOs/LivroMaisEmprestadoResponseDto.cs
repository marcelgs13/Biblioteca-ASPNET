namespace BibliotecaAPI.DTOs;

public class LivroMaisEmprestadoResponseDto
{
    public int LivroId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string AutorNome { get; set; } = string.Empty;
    public int QuantidadeEmprestimos { get; set; }
}
