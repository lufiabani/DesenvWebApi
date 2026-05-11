// Controllers/DetalhesProdutoController.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DesenvWebApi.Api.Data;
using DesenvWebApi.Api.Models;

namespace DesenvWebApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DetalhesProdutoController : ControllerBase
{
    private readonly AppDbContext _context;

    public DetalhesProdutoController(AppDbContext context)
    {
        _context = context;
    }

    // =====================================================================
    // GET /api/detalhesproduto/produto/5
    // Busca o detalhe de um produto específico pelo ID DO PRODUTO.
    //
    // Note a rota especial: "produto/{produtoId}"
    // Isso é mais intuitivo do que buscar pelo ID do detalhe, porque
    // no frontend nós sabemos o ID do produto (está na tabela),
    // mas não sabemos o ID do detalhe (é um dado interno).
    //
    // Exemplo: "Quero ver os detalhes do Notebook (produto ID 1)"
    //   → GET /api/detalhesproduto/produto/1
    //
    // Se o produto não tem detalhe, retorna 404.
    // No frontend, tratamos o 404 como "ainda não tem detalhe"
    // (diferente de um erro real).
    // =====================================================================
    [HttpGet("produto/{produtoId}")]
    public async Task<ActionResult<DetalheProduto>> GetDetalhePorProduto(int produtoId)
    {
        // Busca o detalhe pela FK (ProdutoId), não pela PK (Id).
        // Include carrega o objeto Produto junto na resposta.
        var detalhe = await _context.DetalhesProduto
            .Include(d => d.Produto)
            .FirstOrDefaultAsync(d => d.ProdutoId == produtoId);

        if (detalhe == null)
        {
            return NotFound(new
            {
                mensagem = $"Nenhum detalhe encontrado para o produto de ID {produtoId}."
            });
        }

        return Ok(detalhe);
    }

    // =====================================================================
    // POST /api/detalhesproduto
    // Cria um detalhe para um produto.
    //
    // O body deve conter o ProdutoId do produto ao qual o detalhe pertence.
    //
    // Exemplo de body JSON:
    // {
    //   "especificacoes": "CPU: i7-13700H, RAM: 16GB DDR5",
    //   "garantia": "1 ano pelo fabricante",
    //   "paisDeOrigem": "China",
    //   "pesoGramas": 1850,
    //   "produtoId": 1
    // }
    //
    // VALIDAÇÕES ESPECIAIS para 1-para-1:
    //   1. O produto referenciado deve existir
    //   2. O produto NÃO deve já ter um detalhe cadastrado
    // =====================================================================
    [HttpPost]
    public async Task<ActionResult<DetalheProduto>> PostDetalhe(DetalheProduto detalhe)
    {
        // VALIDAÇÃO 1: O produto existe?
        // Se o ProdutoId não corresponde a nenhum produto,
        // retornamos 400 Bad Request com mensagem clara.
        var produto = await _context.Produtos.FindAsync(detalhe.ProdutoId);
        if (produto == null)
        {
            return BadRequest(new
            {
                mensagem = $"Produto com ID {detalhe.ProdutoId} não encontrado."
            });
        }

        // VALIDAÇÃO 2: O produto já tem um detalhe?
        // Como o relacionamento é 1-para-1, não pode ter dois detalhes
        // para o mesmo produto. Se já existe, informamos o usuário.
        //
        // Por que validar no código e não deixar o banco rejeitar?
        //   - A mensagem de erro do banco é técnica e confusa para o usuário
        //     ("23505: duplicate key value violates unique constraint")
        //   - Validando aqui, retornamos uma mensagem clara e amigável
        //   - O código HTTP 409 (Conflict) é semanticamente correto
        var detalheExistente = await _context.DetalhesProduto
            .FirstOrDefaultAsync(d => d.ProdutoId == detalhe.ProdutoId);

        if (detalheExistente != null)
        {
            return Conflict(new
            {
                mensagem = $"O produto de ID {detalhe.ProdutoId} já possui um detalhe cadastrado. " +
                           "Use PUT para atualizar o detalhe existente."
            });
        }

        _context.DetalhesProduto.Add(detalhe);
        await _context.SaveChangesAsync();

        // Retorna HTTP 201 com Location apontando para o GET do detalhe
        return CreatedAtAction(
            nameof(GetDetalhePorProduto),
            new { produtoId = detalhe.ProdutoId },
            detalhe
        );
    }

    // =====================================================================
    // PUT /api/detalhesproduto/5
    // Atualiza um detalhe pelo ID DO DETALHE (não do produto).
    //
    // Por que pelo ID do detalhe e não do produto?
    // Convenção REST: PUT opera sobre o recurso identificado pela URL.
    // O ID do detalhe é a PK do recurso DetalheProduto.
    //
    // Exemplo de body JSON:
    // {
    //   "id": 1,
    //   "especificacoes": "CPU: i9-14900H, RAM: 32GB DDR5",
    //   "garantia": "2 anos pelo fabricante",
    //   "paisDeOrigem": "China",
    //   "pesoGramas": 1900,
    //   "produtoId": 1
    // }
    // =====================================================================
    [HttpPut("{id}")]
    public async Task<IActionResult> PutDetalhe(int id, DetalheProduto detalhe)
    {
        if (id != detalhe.Id)
        {
            return BadRequest(new { mensagem = "O ID da URL não corresponde ao ID do detalhe." });
        }

        var detalheExistente = await _context.DetalhesProduto.FindAsync(id);

        if (detalheExistente == null)
        {
            return NotFound(new { mensagem = $"Detalhe com ID {id} não encontrado." });
        }

        // Atualiza apenas os campos de conteúdo.
        // NÃO atualizamos o ProdutoId — um detalhe não pode "migrar"
        // para outro produto. Se precisar, delete e crie um novo.
        detalheExistente.Especificacoes = detalhe.Especificacoes;
        detalheExistente.Garantia = detalhe.Garantia;
        detalheExistente.PaisDeOrigem = detalhe.PaisDeOrigem;
        detalheExistente.PesoGramas = detalhe.PesoGramas;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // =====================================================================
    // DELETE /api/detalhesproduto/5
    // Remove um detalhe pelo ID.
    //
    // Diferente da deleção de Categoria (que pode falhar se tiver produtos),
    // a deleção de DetalheProduto é sempre segura — não há outras entidades
    // que dependam do detalhe.
    // =====================================================================
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDetalhe(int id)
    {
        var detalhe = await _context.DetalhesProduto.FindAsync(id);

        if (detalhe == null)
        {
            return NotFound(new { mensagem = $"Detalhe com ID {id} não encontrado." });
        }

        _context.DetalhesProduto.Remove(detalhe);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
