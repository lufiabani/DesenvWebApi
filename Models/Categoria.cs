// Models/Categoria.cs

namespace DesenvWebApi.Api.Models;

// A classe Categoria representa a tabela "Categorias" no banco de dados.
// Esta entidade está no lado "um" do relacionamento 1-para-N com Produtos.
//
// Uma Categoria pode ter muitos Produtos associados.
// Cada Produto pertence a exatamente uma Categoria.
public class Categoria
{
    // Chave primária — identificador único da categoria.
    // O EF reconhece "Id" automaticamente como PK (convenção de nomenclatura).
    // No banco: coluna "Id" INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY
    public int Id { get; set; }

    // Nome da categoria — campo obrigatório.
    // "required" garante que o C# não permite criar uma Categoria sem Nome.
    // No banco: coluna "Nome" TEXT NOT NULL
    // Exemplos: "Eletrônicos", "Móveis", "Alimentos"
    public required string Nome { get; set; }

    // Descrição da categoria — campo opcional.
    // "?" indica que a propriedade pode ser nula (nullable).
    // No banco: coluna "Descricao" TEXT NULL
    public string? Descricao { get; set; }

    // =====================================================================
    // PROPRIEDADE DE NAVEGAÇÃO (lado "um" do relacionamento)
    //
    // Esta propriedade NÃO corresponde a uma coluna no banco de dados.
    // Ela é usada pelo Entity Framework para representar o relacionamento
    // e permite navegar de uma Categoria para seus Produtos diretamente no código.
    //
    // ICollection<Produto> representa uma coleção (lista) de produtos.
    // O EF usa isso para saber que "uma Categoria tem muitos Produtos".
    //
    // Inicializado como lista vazia para evitar NullReferenceException
    // ao acessar a coleção antes de carregar os dados do banco.
    //
    // Uso no código:
    //   categoria.Produtos.Count          → quantos produtos tem
    //   foreach(var p in categoria.Produtos)  → iterar pelos produtos
    //
    // IMPORTANTE: esta coleção só é preenchida quando usamos
    // .Include(c => c.Produtos) na query. Sem isso, será uma lista vazia.
    // =====================================================================
    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}
