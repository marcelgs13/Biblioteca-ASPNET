# Biblioteca API

API REST para gerenciamento de uma biblioteca, desenvolvida com ASP.NET Core. O projeto permite cadastrar e consultar autores, livros e alunos, além de controlar empréstimos, devoluções e a quantidade disponível de cada livro.

## Tecnologias

- C# e .NET 10
- ASP.NET Core Web API
- Entity Framework Core 10
- SQLite
- OpenAPI 3.1 e Swagger UI
- Git

## Arquitetura

O projeto separa as responsabilidades nas seguintes pastas:

```text
BibliotecaAPI/
├── Controllers/    # Rotas e respostas HTTP
├── Data/           # DbContext e mapeamento do EF Core
├── DTOs/           # Contratos de entrada e saída da API
├── Exceptions/     # Exceções e tratamento global com ProblemDetails
├── Migrations/     # Histórico de evolução do banco de dados
├── Models/         # Entidades de domínio
├── Repositories/   # Acesso centralizado aos dados por interface
└── Services/       # Regras de negócio centralizadas por interface
```

O fluxo principal de uma requisição é:

```text
Cliente → Controller → Service → Repository → DbContext → SQLite
```

Para facilitar a leitura do fluxo, o projeto utiliza uma interface e uma implementação
centralizada em `Services` e outra em `Repositories`.

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git

## Como executar

Clone o repositório e entre na pasta do projeto:

```powershell
git clone https://github.com/marcelgs13/Biblioteca-ASPNET.git
cd Biblioteca-ASPNET
```

Restaure as dependências e a ferramenta local do Entity Framework Core:

```powershell
dotnet restore
dotnet tool restore
```

Crie ou atualize o banco SQLite usando as migrations:

```powershell
dotnet tool run dotnet-ef database update
```

Inicie a aplicação com o perfil HTTP:

```powershell
dotnet run --launch-profile http
```

A API ficará disponível em `http://localhost:5293`. A interface Swagger pode ser acessada em:

```text
http://localhost:5293/swagger
```

## Endpoints

| Método | Endpoint | Descrição |
|---|---|---|
| `POST` | `/api/autores` | Cadastra um autor |
| `GET` | `/api/autores` | Lista os autores |
| `GET` | `/api/autores/{id}` | Consulta um autor por ID |
| `PUT` | `/api/autores/{id}` | Atualiza completamente um autor |
| `DELETE` | `/api/autores/{id}` | Exclui um autor sem livros vinculados |
| `POST` | `/api/livros` | Cadastra um livro associado a um autor |
| `GET` | `/api/livros` | Lista livros e aceita os filtros `titulo` e `autor` |
| `GET` | `/api/livros/{id}` | Consulta um livro por ID |
| `POST` | `/api/alunos` | Cadastra um aluno |
| `GET` | `/api/alunos` | Lista os alunos |
| `GET` | `/api/alunos/{id}` | Consulta um aluno por ID |
| `DELETE` | `/api/alunos/{id}` | Exclui um aluno sem empréstimos vinculados |
| `POST` | `/api/emprestimos` | Registra um empréstimo |
| `GET` | `/api/emprestimos` | Lista os empréstimos |
| `GET` | `/api/emprestimos/{id}` | Consulta um empréstimo por ID |
| `PUT` | `/api/emprestimos/{id}/devolucao` | Registra a devolução de um empréstimo |

Exemplos de filtros:

```http
GET /api/livros?titulo=clean
GET /api/livros?autor=martin
GET /api/livros?titulo=clean&autor=martin
```

## Regras de negócio

- A matrícula do aluno deve ser única.
- O autor informado no cadastro de um livro deve existir.
- O aluno e o livro informados em um empréstimo devem existir.
- Um livro sem exemplares disponíveis não pode ser emprestado.
- Um aluno não pode manter dois empréstimos ativos do mesmo livro.
- Ao emprestar um livro, a quantidade disponível é reduzida em uma unidade.
- O prazo de devolução é definido em sete dias a partir do empréstimo.
- Ao devolver um livro, a quantidade disponível é incrementada em uma unidade.
- Um empréstimo já devolvido não pode ser devolvido novamente.
- Autores com livros e alunos com empréstimos não podem ser excluídos.

## Respostas HTTP

| Código | Uso |
|---|---|
| `200 OK` | Consultas e devoluções realizadas com sucesso |
| `201 Created` | Cadastros realizados com sucesso |
| `400 Bad Request` | Dados de entrada inválidos |
| `404 Not Found` | Autor, livro, aluno ou empréstimo inexistente |
| `409 Conflict` | Violação de regra de negócio |
| `500 Internal Server Error` | Erro inesperado no servidor |

Erros de negócio e recursos inexistentes são retornados no padrão `ProblemDetails`. Validações de entrada utilizam `ValidationProblemDetails`.

## Banco de dados

O arquivo `biblioteca.db` é criado localmente pelo comando de migrations e não é versionado. Cada ambiente possui seu próprio banco e seus próprios dados.

O repositório contém a migration inicial necessária para criar as tabelas, chaves estrangeiras e o índice único de matrícula.

## Testes manuais

As operações podem ser testadas de duas formas:

- Pela interface Swagger em `http://localhost:5293/swagger`.
- Pelo arquivo `BibliotecaAPI.http`, usando um cliente HTTP compatível no editor.

Para testar o fluxo completo, cadastre os recursos nesta ordem:

1. Autor.
2. Livro usando o ID do autor.
3. Aluno.
4. Empréstimo usando os IDs do aluno e do livro.
5. Devolução usando o ID do empréstimo.

Os desafios bônus de paginação, autenticação JWT, busca avançada e testes unitários não fazem parte do escopo desta versão.
