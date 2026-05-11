// Controllers/CategoriasController.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DesenvWebApi.Api.Data;
using DesenvWebApi.Api.Models;

namespace DesenvWebApi.Api.Controllers;

// [ApiController] habilita comportamentos automáticos:
//   - Validação automática do ModelState
//   - Inferência de [FromBody] para parâmetros complexos
//   - Respostas de erro padronizadas (400 Bad Request automático)
[ApiController]

// [Route("api/[controller]")] define a URL base deste controller.
// [controller] é substituído pelo nome da classe sem "Controller":
//   CategoriasController → api/categorias
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly AppDbContext _context;

    // Injeção de Dependência — o .NET fornece o AppDbContext automaticamente
    public CategoriasController(AppDbContext context)
    {
        _context = context;
    }

    // =====================================================================
    // GET /api/categorias
    // Retorna todas as categorias.
    //
    // Note que aqui NÃO usamos .Include(c => c.Produtos).
    // Por quê? Porque este endpoint é usado para popular seletores
    // em formulários — não precisamos dos produtos, só dos nomes.
    // Carregar dados desnecessários desperdiça banda e processamento.
    // =====================================================================
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias()
    {
        // SELECT * FROM "Categorias"
        var categorias = await _context.Categorias.ToListAsync();
        return Ok(categorias);
    }

    // =====================================================================
    // GET /api/categorias/5
    // Retorna uma categoria específica COM todos os seus produtos.
    //
    // Aqui SIM usamos .Include() porque quando o usuário clica em uma
    // categoria, ele quer ver quais produtos pertencem a ela.
    // =====================================================================
    [HttpGet("{id}")]
    public async Task<ActionResult<Categoria>> GetCategoria(int id)
    {
        // .Include(c => c.Produtos) instrui o EF a fazer um JOIN:
        //
        //   SELECT c.*, p.*
        //   FROM "Categorias" c
        //   LEFT JOIN "Produtos" p ON p."CategoriaId" = c."Id"
        //   WHERE c."Id" = @id
        //
        // "Eager Loading" = carregamento antecipado.
        // Os dados relacionados são carregados NA MESMA QUERY.
        //
        // Sem Include: categoria.Produtos seria uma lista vazia [].
        // Com Include: categoria.Produtos contém os objetos Produto.
        //
        // LEFT JOIN (e não INNER JOIN) porque queremos retornar a
        // categoria mesmo que ela não tenha nenhum produto.
        var categoria = await _context.Categorias
            .Include(c => c.Produtos)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria == null)
        {
            return NotFound(new { mensagem = $"Categoria com ID {id} não encontrada." });
        }

        return Ok(categoria);
    }

    // =====================================================================
    // POST /api/categorias
    // Cria uma nova categoria.
    //
    // Exemplo de body JSON:
    // {
    //   "nome": "Eletrônicos",
    //   "descricao": "Produtos de tecnologia"
    // }
    // =====================================================================
    [HttpPost]
    public async Task<ActionResult<Categoria>> PostCategoria(Categoria categoria)
    {
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        // Retorna HTTP 201 Created com header Location apontando para
        // GET /api/categorias/{id} do recurso criado.
        return CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id }, categoria);
    }

    // =====================================================================
    // PUT /api/categorias/5
    // Atualiza uma categoria existente.
    //
    // Exemplo de body JSON:
    // {
    //   "id": 5,
    //   "nome": "Tecnologia",
    //   "descricao": "Produtos de tecnologia e eletrônicos"
    // }
    // =====================================================================
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCategoria(int id, Categoria categoria)
    {
        if (id != categoria.Id)
        {
            return BadRequest(new { mensagem = "O ID da URL não corresponde ao ID da categoria." });
        }

        // Buscamos a categoria existente no banco ANTES de atualizar.
        // Isso garante que estamos atualizando um registro que realmente existe.
        var categoriaExistente = await _context.Categorias.FindAsync(id);

        if (categoriaExistente == null)
        {
            return NotFound(new { mensagem = $"Categoria com ID {id} não encontrada." });
        }

        // Atualizamos apenas os campos editáveis.
        // Não alteramos o Id (chave primária nunca muda).
        categoriaExistente.Nome = categoria.Nome;
        categoriaExistente.Descricao = categoria.Descricao;

        await _context.SaveChangesAsync();

        // HTTP 204 — atualização bem-sucedida, sem body de resposta.
        return NoContent();
    }

    // =====================================================================
    // DELETE /api/categorias/5
    // Remove uma categoria pelo ID.
    //
    // IMPORTANTE: Com DeleteBehavior.Restrict configurado no DbContext,
    // o banco NÃO permite deletar uma categoria que ainda tem produtos.
    // O try/catch captura esse erro e retorna uma mensagem amigável.
    //
    // Fluxo:
    //   1. Categoria sem produtos → deletada normalmente
    //   2. Categoria com produtos → banco lança exceção → retornamos 400
    // =====================================================================
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategoria(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);

        if (categoria == null)
        {
            return NotFound(new { mensagem = $"Categoria com ID {id} não encontrada." });
        }

        // O try/catch é necessário aqui por causa do DeleteBehavior.Restrict.
        // Quando tentamos deletar uma categoria que tem produtos associados,
        // o PostgreSQL lança um erro de violação de chave estrangeira.
        //
        // Sem try/catch, o erro "quebraria" a API retornando HTTP 500.
        // Com try/catch, capturamos o erro e retornamos uma mensagem clara.
        try
        {
            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // DbUpdateException é lançada quando o banco recusa a operação.
            // Neste caso, porque há produtos referenciando esta categoria.
            return BadRequest(new
            {
                mensagem = "Não é possível deletar esta categoria porque ela possui produtos associados. " +
                           "Remova ou reatribua os produtos antes de deletar a categoria."
            });
        }

        return NoContent();
    }
}
