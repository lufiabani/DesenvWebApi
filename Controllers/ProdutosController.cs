// Controllers/ProdutosController.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DesenvWebApi.Api.Data;
using DesenvWebApi.Api.Models;

namespace DesenvWebApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProdutosController(AppDbContext context)
    {
        _context = context;
    }

    // =====================================================================
    // GET /api/produtos — agora inclui a Categoria de cada produto
    // =====================================================================
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
    {
        // ANTES (sem relacionamento):
        //   var produtos = await _context.Produtos.ToListAsync();
        //   → SQL: SELECT * FROM "Produtos"
        //   → produto.Categoria seria null
        //
        // DEPOIS (com Include):
        //   → SQL: SELECT p.*, c.* FROM "Produtos" p
        //          LEFT JOIN "Categorias" c ON c."Id" = p."CategoriaId"
        //   → produto.Categoria é o objeto completo { Id, Nome, Descricao }
        //
        // .Include(p => p.Categoria) é o "Eager Loading":
        // instrui o EF a carregar a entidade relacionada NA MESMA QUERY.
        var produtos = await _context.Produtos
            .Include(p => p.Categoria)
            .ToListAsync();

        return Ok(produtos);
    }

    // =====================================================================
    // GET /api/produtos/5 — inclui a Categoria do produto
    // =====================================================================
    [HttpGet("{id}")]
    public async Task<ActionResult<Produto>> GetProduto(int id)
    {
        // Note que aqui usamos FirstOrDefaultAsync em vez de FindAsync.
        // FindAsync não suporta .Include() — ele busca diretamente pelo PK
        // sem possibilidade de incluir relacionamentos.
        //
        // FirstOrDefaultAsync permite encadear .Include() e .Where()
        // antes de executar a query.
        var produto = await _context.Produtos
            .Include(p => p.Categoria)
            .Include(p => p.DetalheProduto)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (produto == null)
        {
            return NotFound(new { mensagem = $"Produto com ID {id} não encontrado." });
        }

        return Ok(produto);
    }

    [HttpPost]
    public async Task<ActionResult<Produto>> PostProduto(Produto produto)
    {
        produto.DataCriacao = DateTime.UtcNow;

        _context.Produtos.Add(produto);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduto), new { id = produto.Id }, produto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutProduto([FromRoute] int id, [FromBody] Produto produto)
    {
        if (id != produto.Id)
        {
            return BadRequest(new { mensagem = "O ID da URL não corresponde ao ID do produto no body." });
        }

        var produtoExistente = await _context.Produtos.FindAsync(id);

        if (produtoExistente == null)
        {
            return NotFound(new { mensagem = $"Produto com ID {id} não encontrado." });
        }

        produtoExistente.Nome = produto.Nome;
        produtoExistente.Descricao = produto.Descricao;
        produtoExistente.Preco = produto.Preco;
        produtoExistente.Quantidade = produto.Quantidade;
        produtoExistente.CategoriaId = produto.CategoriaId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduto(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);

        if (produto == null)
        {
            return NotFound(new { mensagem = $"Produto com ID {id} não encontrado." });
        }

        _context.Produtos.Remove(produto);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
