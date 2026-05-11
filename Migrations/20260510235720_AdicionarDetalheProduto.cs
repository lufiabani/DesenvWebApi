using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DesenvWebApi.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDetalheProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DetalhesProduto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Especificacoes = table.Column<string>(type: "text", nullable: true),
                    Garantia = table.Column<string>(type: "text", nullable: true),
                    PaisDeOrigem = table.Column<string>(type: "text", nullable: true),
                    PesoGramas = table.Column<double>(type: "double precision", nullable: true),
                    ProdutoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalhesProduto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetalhesProduto_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetalhesProduto_ProdutoId",
                table: "DetalhesProduto",
                column: "ProdutoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetalhesProduto");
        }
    }
}
