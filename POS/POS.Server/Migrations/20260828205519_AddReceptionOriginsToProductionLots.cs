using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddReceptionOriginsToProductionLots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ORIGEN_MATERIA_PRIMA_LOTE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionLotMaterialDetailId = table.Column<int>(type: "int", nullable: false),
                    ReceptionDetailId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORIGEN_MATERIA_PRIMA_LOTE", x => x.Id);
                    table.CheckConstraint("CK_ORIGEN_MATERIA_PRIMA_LOTE_Cantidad", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_ORIGEN_MATERIA_PRIMA_LOTE_DETALLE_LOTE_MATERIA_PRIMA_ProductionLotMaterialDetailId",
                        column: x => x.ProductionLotMaterialDetailId,
                        principalTable: "DETALLE_LOTE_MATERIA_PRIMA",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ORIGEN_MATERIA_PRIMA_LOTE_DETALLE_RECEPCION_ReceptionDetailId",
                        column: x => x.ReceptionDetailId,
                        principalTable: "DETALLE_RECEPCION",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ORIGEN_MATERIA_PRIMA_LOTE_ProductionLotMaterialDetailId_ReceptionDetailId",
                table: "ORIGEN_MATERIA_PRIMA_LOTE",
                columns: new[] { "ProductionLotMaterialDetailId", "ReceptionDetailId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ORIGEN_MATERIA_PRIMA_LOTE_ReceptionDetailId",
                table: "ORIGEN_MATERIA_PRIMA_LOTE",
                column: "ReceptionDetailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ORIGEN_MATERIA_PRIMA_LOTE");
        }
    }
}
