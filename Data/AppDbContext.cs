// Data/AppDbContext.cs — versão atualizada com DetalheProduto

using Microsoft.EntityFrameworkCore;
using DesenvWebApi.Api.Models;

namespace DesenvWebApi.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Categoria> Categorias { get; set; }

    // =====================================================================
    // NOVO: DbSet para DetalheProduto
    //
    // O EF vai criar (ou verificar) uma tabela "DetalhesProduto" no banco.
    // Note o nome no plural: a convenção do DbSet define o nome da tabela.
    // =====================================================================
    public DbSet<DetalheProduto> DetalhesProduto { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // =================================================================
        // RELACIONAMENTO 1-PARA-N: Produto → Categoria (do módulo anterior)
        // =================================================================
        modelBuilder.Entity<Produto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Produtos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // =================================================================
        // RELACIONAMENTO 1-PARA-1: Produto → DetalheProduto (NOVO)
        //
        // Leia da seguinte forma:
        //   "Um DetalheProduto TEM UM Produto"
        //   "Um Produto TEM NO MÁXIMO UM DetalheProduto"
        //   "A chave estrangeira está em DetalheProduto (ProdutoId)"
        //   "Se o Produto for deletado, o DetalheProduto também é deletado"
        // =================================================================
        modelBuilder.Entity<DetalheProduto>()
            // Um DetalheProduto tem um Produto (navegação)
            .HasOne(d => d.Produto)
            // Um Produto pode ter no máximo um DetalheProduto (navegação inversa)
            // Note: WithOne (não WithMany!) — essa é a diferença para 1-para-N
            .WithOne(p => p.DetalheProduto)
            // A chave estrangeira está na tabela DetalheProduto.
            // <DetalheProduto> é necessário para o EF saber em qual tabela
            // está a FK (ambiguidade: a FK poderia estar em qualquer lado).
            .HasForeignKey<DetalheProduto>(d => d.ProdutoId)
            // Se o Produto for deletado, o DetalheProduto é deletado junto.
            //
            // Usamos Cascade aqui (diferente do Restrict na Categoria) porque:
            //   - Um DetalheProduto sem Produto não faz sentido
            //   - É seguro: deletar 1 detalhe não é perigoso como deletar N produtos
            //   - É prático: o usuário não precisa deletar o detalhe manualmente
            //     antes de deletar o produto
            .OnDelete(DeleteBehavior.Cascade);

        // =================================================================
        // RESTRIÇÃO UNIQUE em ProdutoId
        //
        // ESTA É A LINHA QUE GARANTE O 1-PARA-1!
        //
        // Sem ela, o banco permitiria múltiplos DetalheProduto com o mesmo
        // ProdutoId — e o relacionamento seria 1-para-N.
        //
        // Com o índice UNIQUE, qualquer tentativa de INSERT com um ProdutoId
        // que já existe na tabela será rejeitada pelo banco:
        //   "23505: duplicate key value violates unique constraint"
        //
        // Índice UNIQUE também melhora a performance de buscas por ProdutoId.
        // =================================================================
        modelBuilder.Entity<DetalheProduto>()
            .HasIndex(d => d.ProdutoId)
            .IsUnique();
    }
}
