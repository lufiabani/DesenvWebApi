// Models/DetalheProduto.cs

namespace DesenvWebApi.Api.Models;

// DetalheProduto representa informações técnicas estendidas de um Produto.
// Existe em uma relação 1-para-1 com Produto:
//   - Um Produto pode ter NO MÁXIMO um DetalheProduto
//   - Um DetalheProduto pertence a EXATAMENTE um Produto
//
// Por que separar em outra tabela e não adicionar campos em Produto?
//   1. Separação de responsabilidades: a tabela Produto fica leve e rápida
//   2. Nem todo produto terá detalhes — evitamos muitas colunas nulas
//   3. Dados de detalhe são acessados raramente (só na página de detalhe)
//   4. Facilita a manutenção: se precisar adicionar mais campos de detalhe,
//      não "poluímos" a tabela principal
public class DetalheProduto
{
    // Chave primária do detalhe.
    // Note que NÃO é o mesmo que o ProdutoId.
    // DetalheProduto tem seu próprio Id independente.
    public int Id { get; set; }

    // Especificações técnicas do produto.
    // Exemplo: "Processador: i7 13ª geração, RAM: 16GB DDR5, SSD: 512GB NVMe"
    // Campo opcional — nem todo detalhe precisa de especificações.
    public string? Especificacoes { get; set; }

    // Informações de garantia do produto.
    // Exemplo: "1 ano pelo fabricante + 90 dias adicionais pela loja"
    public string? Garantia { get; set; }

    // País onde o produto foi fabricado.
    // Exemplo: "China", "Brasil", "Alemanha"
    public string? PaisDeOrigem { get; set; }

    // Peso do produto em gramas.
    // double? porque nem todo produto tem peso informado.
    // Usamos double em vez de decimal porque peso não é monetário
    // e não precisa da precisão exata do decimal.
    // Exemplo: 1850.5 (gramas)
    public double? PesoGramas { get; set; }

    // =====================================================================
    // CHAVE ESTRANGEIRA do relacionamento 1-para-1
    //
    // ProdutoId referencia o Id da tabela Produtos.
    //
    // O que torna esse relacionamento 1-para-1 (e não 1-para-N) é a
    // restrição UNIQUE que adicionaremos no DbContext.
    //
    // Com UNIQUE:  cada ProdutoId pode aparecer NO MÁXIMO UMA VEZ
    //              na tabela DetalheProdutos.
    // Sem UNIQUE:  o mesmo ProdutoId poderia aparecer múltiplas vezes,
    //              tornando o relacionamento 1-para-N.
    //
    // É "int" (não "int?") porque um detalhe DEVE pertencer a um produto.
    // Um DetalheProduto sem Produto não faz sentido.
    // =====================================================================
    public int ProdutoId { get; set; }

    // Propriedade de navegação para o Produto relacionado.
    // Permite acessar: detalhe.Produto.Nome, detalhe.Produto.Preco, etc.
    // Só é preenchida quando usamos .Include(d => d.Produto) na query.
    public Produto? Produto { get; set; }
}
