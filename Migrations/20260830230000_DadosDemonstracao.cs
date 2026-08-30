using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotecaAPI.Migrations
{
    /// <inheritdoc />
    public partial class DadosDemonstracao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                INSERT OR IGNORE INTO Alunos (Nome, Matricula, Email)
                SELECT 'José Miguel', '20232ewbj0030', 'aluno@ifpe.edu.br'
                WHERE NOT EXISTS (SELECT 1 FROM Alunos WHERE Email = 'aluno@ifpe.edu.br');

                INSERT OR IGNORE INTO Autores (Nome, DataNascimento, Nacionalidade)
                SELECT 'George Orwell', '1903-06-25 00:00:00', 'Britânico'
                WHERE NOT EXISTS (SELECT 1 FROM Autores WHERE Nome = 'George Orwell');
                INSERT OR IGNORE INTO Autores (Nome, DataNascimento, Nacionalidade)
                SELECT 'Clarice Lispector', '1920-12-10 00:00:00', 'Brasileira'
                WHERE NOT EXISTS (SELECT 1 FROM Autores WHERE Nome = 'Clarice Lispector');
                INSERT OR IGNORE INTO Autores (Nome, DataNascimento, Nacionalidade)
                SELECT 'Machado de Assis', '1839-06-21 00:00:00', 'Brasileiro'
                WHERE NOT EXISTS (SELECT 1 FROM Autores WHERE Nome = 'Machado de Assis');
                INSERT OR IGNORE INTO Autores (Nome, DataNascimento, Nacionalidade)
                SELECT 'Antoine de Saint-Exupéry', '1900-06-29 00:00:00', 'Francês'
                WHERE NOT EXISTS (SELECT 1 FROM Autores WHERE Nome = 'Antoine de Saint-Exupéry');
                INSERT OR IGNORE INTO Autores (Nome, DataNascimento, Nacionalidade)
                SELECT 'Robert C. Martin', '1952-12-05 00:00:00', 'Americano'
                WHERE NOT EXISTS (SELECT 1 FROM Autores WHERE Nome = 'Robert C. Martin');

                INSERT OR IGNORE INTO Livros
                    (ISBN, Titulo, Descricao, AnoPublicacao, Editora, Categoria, Quantidade, Localizacao, AutorId)
                SELECT '9788535914849', '1984',
                    'Uma distopia sobre vigilância, autoritarismo e controle da informação.',
                    1949, 'Companhia das Letras', 'Distopia', 1, 'Estante A - Prateleira 1',
                    (SELECT Id FROM Autores WHERE Nome = 'George Orwell' ORDER BY Id LIMIT 1)
                WHERE NOT EXISTS (SELECT 1 FROM Livros WHERE ISBN = '9788535914849');

                INSERT OR IGNORE INTO Livros
                    (ISBN, Titulo, Descricao, AnoPublicacao, Editora, Categoria, Quantidade, Localizacao, AutorId)
                SELECT '9788532508126', 'A Hora da Estrela',
                    'A trajetória de Macabéa e sua busca por identidade em uma grande cidade.',
                    1977, 'Rocco', 'Romance', 1, 'Estante B - Prateleira 1',
                    (SELECT Id FROM Autores WHERE Nome = 'Clarice Lispector' ORDER BY Id LIMIT 1)
                WHERE NOT EXISTS (SELECT 1 FROM Livros WHERE ISBN = '9788532508126');

                INSERT OR IGNORE INTO Livros
                    (ISBN, Titulo, Descricao, AnoPublicacao, Editora, Categoria, Quantidade, Localizacao, AutorId)
                SELECT '9788594318602', 'Dom Casmurro',
                    'Memórias de Bentinho marcadas por amor, ciúme e dúvidas sobre o passado.',
                    1899, 'Principis', 'Romance brasileiro', 2, 'Estante B - Prateleira 2',
                    (SELECT Id FROM Autores WHERE Nome = 'Machado de Assis' ORDER BY Id LIMIT 1)
                WHERE NOT EXISTS (SELECT 1 FROM Livros WHERE ISBN = '9788594318602');

                INSERT OR IGNORE INTO Livros
                    (ISBN, Titulo, Descricao, AnoPublicacao, Editora, Categoria, Quantidade, Localizacao, AutorId)
                SELECT '9788595084759', 'Memórias Póstumas de Brás Cubas',
                    'Um narrador defunto revisita sua vida com ironia e crítica social.',
                    1881, 'Nova Fronteira', 'Romance brasileiro', 0, 'Estante B - Prateleira 3',
                    (SELECT Id FROM Autores WHERE Nome = 'Machado de Assis' ORDER BY Id LIMIT 1)
                WHERE NOT EXISTS (SELECT 1 FROM Livros WHERE ISBN = '9788595084759');

                INSERT OR IGNORE INTO Livros
                    (ISBN, Titulo, Descricao, AnoPublicacao, Editora, Categoria, Quantidade, Localizacao, AutorId)
                SELECT '9788595081512', 'O Pequeno Príncipe',
                    'Uma narrativa sobre amizade, responsabilidade e aquilo que é essencial.',
                    1943, 'HarperCollins', 'Infantojuvenil', 3, 'Estante C - Prateleira 1',
                    (SELECT Id FROM Autores WHERE Nome = 'Antoine de Saint-Exupéry' ORDER BY Id LIMIT 1)
                WHERE NOT EXISTS (SELECT 1 FROM Livros WHERE ISBN = '9788595081512');

                INSERT OR IGNORE INTO Livros
                    (ISBN, Titulo, Descricao, AnoPublicacao, Editora, Categoria, Quantidade, Localizacao, AutorId)
                SELECT '9780132350884', 'Clean Code',
                    'Práticas e princípios para desenvolver código limpo e sustentável.',
                    2008, 'Prentice Hall', 'Tecnologia', 1, 'Estante D - Prateleira 1',
                    (SELECT Id FROM Autores WHERE Nome = 'Robert C. Martin' ORDER BY Id LIMIT 1)
                WHERE NOT EXISTS (SELECT 1 FROM Livros WHERE ISBN = '9780132350884');

                INSERT OR IGNORE INTO Livros
                    (ISBN, Titulo, Descricao, AnoPublicacao, Editora, Categoria, Quantidade, Localizacao, AutorId)
                SELECT '9788535916806', 'A Revolução dos Bichos',
                    'Uma fábula política sobre poder, desigualdade e corrupção.',
                    1945, 'Companhia das Letras', 'Sátira', 2, 'Estante A - Prateleira 2',
                    (SELECT Id FROM Autores WHERE Nome = 'George Orwell' ORDER BY Id LIMIT 1)
                WHERE NOT EXISTS (SELECT 1 FROM Livros WHERE ISBN = '9788535916806');

                INSERT OR IGNORE INTO Emprestimos
                    (AlunoId, LivroId, DataEmprestimo, DataPrevistaDevolucao, DataDevolucao, Status)
                SELECT aluno.Id, livro.Id, datetime('now', '-2 days'), datetime('now', '+5 days'), NULL, 0
                FROM Alunos aluno, Livros livro
                WHERE aluno.Email = 'aluno@ifpe.edu.br' AND livro.ISBN = '9788595081512'
                  AND NOT EXISTS (
                    SELECT 1 FROM Emprestimos existente
                    WHERE existente.AlunoId = aluno.Id AND existente.LivroId = livro.Id
                      AND existente.Status IN (0, 2));

                INSERT OR IGNORE INTO Emprestimos
                    (AlunoId, LivroId, DataEmprestimo, DataPrevistaDevolucao, DataDevolucao, Status)
                SELECT aluno.Id, livro.Id, datetime('now', '-18 days'), datetime('now', '-11 days'), NULL, 2
                FROM Alunos aluno, Livros livro
                WHERE aluno.Email = 'aluno@ifpe.edu.br' AND livro.ISBN = '9788535914849'
                  AND NOT EXISTS (
                    SELECT 1 FROM Emprestimos existente
                    WHERE existente.AlunoId = aluno.Id AND existente.LivroId = livro.Id
                      AND existente.Status IN (0, 2));

                INSERT OR IGNORE INTO Emprestimos
                    (AlunoId, LivroId, DataEmprestimo, DataPrevistaDevolucao, DataDevolucao, Status)
                SELECT aluno.Id, livro.Id, datetime('now', '-29 days'), datetime('now', '-22 days'), NULL, 2
                FROM Alunos aluno, Livros livro
                WHERE aluno.Email = 'aluno@ifpe.edu.br' AND livro.ISBN = '9788594318602'
                  AND NOT EXISTS (
                    SELECT 1 FROM Emprestimos existente
                    WHERE existente.AlunoId = aluno.Id AND existente.LivroId = livro.Id
                      AND existente.Status IN (0, 2));

                INSERT OR IGNORE INTO Emprestimos
                    (AlunoId, LivroId, DataEmprestimo, DataPrevistaDevolucao, DataDevolucao, Status)
                SELECT aluno.Id, livro.Id, datetime('now', '-50 days'), datetime('now', '-43 days'), datetime('now', '-40 days'), 1
                FROM Alunos aluno, Livros livro
                WHERE aluno.Email = 'aluno@ifpe.edu.br' AND livro.ISBN = '9788532508126';

                INSERT OR IGNORE INTO Emprestimos
                    (AlunoId, LivroId, DataEmprestimo, DataPrevistaDevolucao, DataDevolucao, Status)
                SELECT aluno.Id, livro.Id, datetime('now', '-60 days'), datetime('now', '-53 days'), datetime('now', '-54 days'), 1
                FROM Alunos aluno, Livros livro
                WHERE aluno.Email = 'aluno@ifpe.edu.br' AND livro.ISBN = '9780132350884';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Os dados são preservados no downgrade para não apagar registros que já tenham sido utilizados.
        }
    }
}
