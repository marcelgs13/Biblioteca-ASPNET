using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotecaAPI.Migrations
{
    /// <inheritdoc />
    public partial class CorrigeIdsDadosDemonstracao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("PRAGMA foreign_keys = 0;", suppressTransaction: true);

            migrationBuilder.Sql(
                """
                UPDATE Alunos SET Id = -Id WHERE Id = -1000;
                UPDATE Usuarios SET AlunoId = -AlunoId WHERE AlunoId = -1000;
                UPDATE Emprestimos SET AlunoId = -AlunoId WHERE AlunoId = -1000;
                UPDATE Reservas SET AlunoId = -AlunoId WHERE AlunoId = -1000;
                UPDATE Notificacoes SET AlunoId = -AlunoId WHERE AlunoId = -1000;

                UPDATE Autores SET Id = -Id WHERE Id BETWEEN -1005 AND -1001;
                UPDATE Livros SET AutorId = -AutorId WHERE AutorId BETWEEN -1005 AND -1001;

                UPDATE Livros SET Id = -Id WHERE Id BETWEEN -2007 AND -2001;
                UPDATE Emprestimos SET LivroId = -LivroId WHERE LivroId BETWEEN -2007 AND -2001;
                UPDATE Reservas SET LivroId = -LivroId WHERE LivroId BETWEEN -2007 AND -2001;

                UPDATE Emprestimos SET Id = -Id WHERE Id BETWEEN -3005 AND -3001;
                """);

            migrationBuilder.Sql("PRAGMA foreign_keys = 1;", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Não converte os IDs novamente para valores negativos.
        }
    }
}
