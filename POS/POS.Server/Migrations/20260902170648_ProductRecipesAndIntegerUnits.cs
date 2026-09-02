using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Server.Migrations
{
    /// <inheritdoc />
    public partial class ProductRecipesAndIntegerUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM PRODUCTO WHERE CurrentStock <> FLOOR(CurrentStock) OR MinimumStock <> FLOOR(MinimumStock))
                    THROW 51000, 'No se puede migrar PRODUCTO: existen existencias fraccionarias.', 1;
                IF EXISTS (SELECT 1 FROM LOTE_PRODUCCION WHERE PlannedQuantity <> FLOOR(PlannedQuantity) OR ProducedQuantity <> FLOOR(ProducedQuantity))
                    THROW 51000, 'No se puede migrar LOTE_PRODUCCION: existen unidades fraccionarias.', 1;
                IF EXISTS (SELECT 1 FROM DETALLE_DESPACHO WHERE Quantity <> FLOOR(Quantity))
                    THROW 51000, 'No se puede migrar DETALLE_DESPACHO: existen unidades fraccionarias.', 1;
                """);

            migrationBuilder.DropCheckConstraint(name: "CK_PRODUCTO_StockActual", table: "PRODUCTO");
            migrationBuilder.DropCheckConstraint(name: "CK_PRODUCTO_StockMinimo", table: "PRODUCTO");
            migrationBuilder.DropCheckConstraint(name: "CK_LOTE_PRODUCCION_CantidadPlanificada", table: "LOTE_PRODUCCION");
            migrationBuilder.DropCheckConstraint(name: "CK_LOTE_PRODUCCION_CantidadProducida", table: "LOTE_PRODUCCION");
            migrationBuilder.DropCheckConstraint(name: "CK_DETALLE_DESPACHO_Cantidad", table: "DETALLE_DESPACHO");
            migrationBuilder.AlterColumn<int>(
                name: "MinimumStock",
                table: "PRODUCTO",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "CurrentStock",
                table: "PRODUCTO",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "ProducedQuantity",
                table: "LOTE_PRODUCCION",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "PlannedQuantity",
                table: "LOTE_PRODUCCION",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "DETALLE_DESPACHO",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddCheckConstraint(name: "CK_PRODUCTO_StockActual", table: "PRODUCTO", sql: "[CurrentStock] >= 0");
            migrationBuilder.AddCheckConstraint(name: "CK_PRODUCTO_StockMinimo", table: "PRODUCTO", sql: "[MinimumStock] >= 0");
            migrationBuilder.AddCheckConstraint(name: "CK_LOTE_PRODUCCION_CantidadPlanificada", table: "LOTE_PRODUCCION", sql: "[PlannedQuantity] > 0");
            migrationBuilder.AddCheckConstraint(name: "CK_LOTE_PRODUCCION_CantidadProducida", table: "LOTE_PRODUCCION", sql: "[ProducedQuantity] >= 0");
            migrationBuilder.AddCheckConstraint(name: "CK_DETALLE_DESPACHO_Cantidad", table: "DETALLE_DESPACHO", sql: "[Quantity] > 0");

            migrationBuilder.CreateTable(
                name: "RECETA_PRODUCTO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    RawMaterialId = table.Column<int>(type: "int", nullable: false),
                    QuantityPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RECETA_PRODUCTO", x => x.Id);
                    table.CheckConstraint("CK_RECETA_PRODUCTO_Cantidad", "[QuantityPerUnit] > 0");
                    table.ForeignKey(
                        name: "FK_RECETA_PRODUCTO_MATERIA_PRIMA_RawMaterialId",
                        column: x => x.RawMaterialId,
                        principalTable: "MATERIA_PRIMA",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RECETA_PRODUCTO_PRODUCTO_ProductId",
                        column: x => x.ProductId,
                        principalTable: "PRODUCTO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RECETA_PRODUCTO_ProductId_RawMaterialId",
                table: "RECETA_PRODUCTO",
                columns: new[] { "ProductId", "RawMaterialId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RECETA_PRODUCTO_RawMaterialId",
                table: "RECETA_PRODUCTO",
                column: "RawMaterialId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RECETA_PRODUCTO");

            migrationBuilder.DropCheckConstraint(name: "CK_PRODUCTO_StockActual", table: "PRODUCTO");
            migrationBuilder.DropCheckConstraint(name: "CK_PRODUCTO_StockMinimo", table: "PRODUCTO");
            migrationBuilder.DropCheckConstraint(name: "CK_LOTE_PRODUCCION_CantidadPlanificada", table: "LOTE_PRODUCCION");
            migrationBuilder.DropCheckConstraint(name: "CK_LOTE_PRODUCCION_CantidadProducida", table: "LOTE_PRODUCCION");
            migrationBuilder.DropCheckConstraint(name: "CK_DETALLE_DESPACHO_Cantidad", table: "DETALLE_DESPACHO");

            migrationBuilder.AlterColumn<decimal>(
                name: "MinimumStock",
                table: "PRODUCTO",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentStock",
                table: "PRODUCTO",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "ProducedQuantity",
                table: "LOTE_PRODUCCION",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "PlannedQuantity",
                table: "LOTE_PRODUCCION",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "DETALLE_DESPACHO",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddCheckConstraint(name: "CK_PRODUCTO_StockActual", table: "PRODUCTO", sql: "[CurrentStock] >= 0");
            migrationBuilder.AddCheckConstraint(name: "CK_PRODUCTO_StockMinimo", table: "PRODUCTO", sql: "[MinimumStock] >= 0");
            migrationBuilder.AddCheckConstraint(name: "CK_LOTE_PRODUCCION_CantidadPlanificada", table: "LOTE_PRODUCCION", sql: "[PlannedQuantity] > 0");
            migrationBuilder.AddCheckConstraint(name: "CK_LOTE_PRODUCCION_CantidadProducida", table: "LOTE_PRODUCCION", sql: "[ProducedQuantity] >= 0");
            migrationBuilder.AddCheckConstraint(name: "CK_DETALLE_DESPACHO_Cantidad", table: "DETALLE_DESPACHO", sql: "[Quantity] > 0");
        }
    }
}
