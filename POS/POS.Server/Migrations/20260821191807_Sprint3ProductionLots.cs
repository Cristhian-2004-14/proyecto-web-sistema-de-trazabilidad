using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Server.Migrations
{
    /// <inheritdoc />
    public partial class Sprint3ProductionLots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LOTE_PRODUCCION",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProducedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LOTE_PRODUCCION", x => x.Id);
                    table.CheckConstraint("CK_LOTE_PRODUCCION_CantidadPlanificada", "[PlannedQuantity] > 0");
                    table.CheckConstraint("CK_LOTE_PRODUCCION_CantidadProducida", "[ProducedQuantity] >= 0");
                    table.ForeignKey(
                        name: "FK_LOTE_PRODUCCION_PRODUCTO_ProductId",
                        column: x => x.ProductId,
                        principalTable: "PRODUCTO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LOTE_PRODUCCION_USUARIO_UserId",
                        column: x => x.UserId,
                        principalTable: "USUARIO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DETALLE_LOTE_MATERIA_PRIMA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionLotId = table.Column<int>(type: "int", nullable: false),
                    RawMaterialId = table.Column<int>(type: "int", nullable: false),
                    QuantityUsed = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DETALLE_LOTE_MATERIA_PRIMA", x => x.Id);
                    table.CheckConstraint("CK_DETALLE_LOTE_MP_Cantidad", "[QuantityUsed] > 0");
                    table.ForeignKey(
                        name: "FK_DETALLE_LOTE_MATERIA_PRIMA_LOTE_PRODUCCION_ProductionLotId",
                        column: x => x.ProductionLotId,
                        principalTable: "LOTE_PRODUCCION",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DETALLE_LOTE_MATERIA_PRIMA_MATERIA_PRIMA_RawMaterialId",
                        column: x => x.RawMaterialId,
                        principalTable: "MATERIA_PRIMA",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DETALLE_LOTE_MATERIA_PRIMA_ProductionLotId_RawMaterialId",
                table: "DETALLE_LOTE_MATERIA_PRIMA",
                columns: new[] { "ProductionLotId", "RawMaterialId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DETALLE_LOTE_MATERIA_PRIMA_RawMaterialId",
                table: "DETALLE_LOTE_MATERIA_PRIMA",
                column: "RawMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_LOTE_PRODUCCION_Code",
                table: "LOTE_PRODUCCION",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LOTE_PRODUCCION_ProductId",
                table: "LOTE_PRODUCCION",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_LOTE_PRODUCCION_UserId",
                table: "LOTE_PRODUCCION",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DETALLE_LOTE_MATERIA_PRIMA");

            migrationBuilder.DropTable(
                name: "LOTE_PRODUCCION");
        }
    }
}
