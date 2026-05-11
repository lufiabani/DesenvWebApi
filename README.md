# DesenvWebApi

API REST em **.NET 8** com **Entity Framework Core** e **PostgreSQL** para o projeto de Desenvolvimento de Sistemas Web (UFSC): produtos, categorias (1‑N) e detalhes de produto (1‑1).

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL acessível (local ou Docker)
- Ferramenta EF (instalação global ou como ferramenta local):

```bash
dotnet tool install --global dotnet-ef
```

## Banco de dados

### Connection string

Definida em `appsettings.json` (`ConnectionStrings:DefaultConnection`):

- **Host:** `localhost`
- **Porta:** `5433` (mapeada no `docker-compose` da raiz do repositório)
- **Database:** `produtosdb`
- **Usuário / senha:** conforme o compose (`postgres` / `postgres123`)

Ajuste `appsettings.json` (ou variáveis de ambiente) se usar outro servidor.

### Subir o Postgres com Docker

Na **raiz** do repositório:

```bash
docker compose up -d
```

Isso sobe PostgreSQL (e opcionalmente pgAdmin, conforme o `docker-compose.yml`).

### Migrations

Na pasta **deste projeto** (`DesenvWebApi`):

```bash
dotnet ef database update
```

Para criar uma nova migration após alterar modelos:

```bash
dotnet ef migrations add NomeDescritivoDaMigration
dotnet ef database update
```

## Executar a API

```bash
cd DesenvWebApi
dotnet run
```

- Perfil **http:** `http://localhost:5113` (confira no terminal se a porta mudar).
- **Swagger** (desenvolvimento): `http://localhost:5113/swagger`

## Endpoints principais

| Área | Base | Observação |
|------|------|--------------|
| Produtos | `/api/produtos` | GET inclui `categoria`; GET por id inclui `detalheProduto` |
| Categorias | `/api/categorias` | CRUD; delete bloqueado se houver produtos (mensagem JSON) |
| Detalhes | `/api/detalhesproduto` | GET por produto: `GET .../produto/{produtoId}` |

CORS está configurado para permitir o front em outra origem/porta.

## Estrutura do projeto

| Pasta | Conteúdo |
|-------|-----------|
| `Controllers/` | `ProdutosController`, `CategoriasController`, `DetalhesProdutoController` |
| `Models/` | `Produto`, `Categoria`, `DetalheProduto` |
| `Data/` | `AppDbContext` (Fluent API, relacionamentos) |
| `Migrations/` | Histórico EF para o PostgreSQL |

## JSON e ciclos

`Program.cs` configura `ReferenceHandler.IgnoreCycles` para evitar erro de serialização circular entre entidades relacionadas.

---

*Disciplina: Desenvolvimento de Sistemas Web — UFSC*
