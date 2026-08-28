using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddLotDispatchTraceability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductionLotId",
                table: "DETALLE_DESPACHO",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DETALLE_DESPACHO_ProductionLotId",
                table: "DETALLE_DESPACHO",
                column: "ProductionLotId");

            migrationBuilder.AddForeignKey(
                name: "FK_DETALLE_DESPACHO_LOTE_PRODUCCION_ProductionLotId",
                table: "DETALLE_DESPACHO",
                column: "ProductionLotId",
                principalTable: "LOTE_PRODUCCION",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DETALLE_DESPACHO_LOTE_PRODUCCION_ProductionLotId",
                table: "DETALLE_DESPACHO");

            migrationBuilder.DropIndex(
                name: "IX_DETALLE_DESPACHO_ProductionLotId",
                table: "DETALLE_DESPACHO");

            migrationBuilder.DropColumn(
                name: "ProductionLotId",
                table: "DETALLE_DESPACHO");
        }
    }
}
