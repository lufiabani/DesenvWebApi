// Models/Produto.cs — versão final com ambos os relacionamentos

namespace DesenvWebApi.Api.Models;

public class Produto
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public string? Descricao { get; set; }
    public decimal Preco { get; set; }
    public int Quantidade { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    // Relacionamento 1-para-N com Categoria (do módulo anterior)
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    // Relacionamento 1-para-1 com DetalheProduto (NOVO)
    public DetalheProduto? DetalheProduto { get; set; }
}
