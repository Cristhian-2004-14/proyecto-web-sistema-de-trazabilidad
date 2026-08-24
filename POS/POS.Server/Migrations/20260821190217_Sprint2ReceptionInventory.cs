using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Server.Migrations
{
    /// <inheritdoc />
    public partial class Sprint2ReceptionInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PRODUCTO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CurrentStock = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumStock = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRODUCTO", x => x.Id);
                    table.CheckConstraint("CK_PRODUCTO_StockActual", "[CurrentStock] >= 0");
                    table.CheckConstraint("CK_PRODUCTO_StockMinimo", "[MinimumStock] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "RECEPCION",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observation = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RECEPCION", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RECEPCION_PROVEEDOR_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "PROVEEDOR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RECEPCION_USUARIO_UserId",
                        column: x => x.UserId,
                        principalTable: "USUARIO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DETALLE_RECEPCION",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceptionId = table.Column<int>(type: "int", nullable: false),
                    RawMaterialId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DETALLE_RECEPCION", x => x.Id);
                    table.CheckConstraint("CK_DETALLE_RECEPCION_Cantidad", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_DETALLE_RECEPCION_MATERIA_PRIMA_RawMaterialId",
                        column: x => x.RawMaterialId,
                        principalTable: "MATERIA_PRIMA",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DETALLE_RECEPCION_RECEPCION_ReceptionId",
                        column: x => x.ReceptionId,
                        principalTable: "RECEPCION",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DETALLE_RECEPCION_RawMaterialId",
                table: "DETALLE_RECEPCION",
                column: "RawMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_DETALLE_RECEPCION_ReceptionId_RawMaterialId",
                table: "DETALLE_RECEPCION",
                columns: new[] { "ReceptionId", "RawMaterialId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RECEPCION_SupplierId",
                table: "RECEPCION",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_RECEPCION_UserId",
                table: "RECEPCION",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DETALLE_RECEPCION");

            migrationBuilder.DropTable(
                name: "PRODUCTO");

            migrationBuilder.DropTable(
                name: "RECEPCION");
        }
    }
}
