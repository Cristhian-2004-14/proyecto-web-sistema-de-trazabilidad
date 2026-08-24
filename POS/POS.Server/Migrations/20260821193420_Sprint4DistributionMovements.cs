using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Server.Migrations
{
    /// <inheritdoc />
    public partial class Sprint4DistributionMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DESPACHO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Observation = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DESPACHO", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DESPACHO_USUARIO_UserId",
                        column: x => x.UserId,
                        principalTable: "USUARIO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MOVIMIENTO_INVENTARIO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    MovementType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MOVIMIENTO_INVENTARIO", x => x.Id);
                    table.CheckConstraint("CK_MOVIMIENTO_INVENTARIO_Cantidad", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_MOVIMIENTO_INVENTARIO_PRODUCTO_ProductId",
                        column: x => x.ProductId,
                        principalTable: "PRODUCTO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MOVIMIENTO_INVENTARIO_USUARIO_UserId",
                        column: x => x.UserId,
                        principalTable: "USUARIO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DETALLE_DESPACHO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DispatchId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DETALLE_DESPACHO", x => x.Id);
                    table.CheckConstraint("CK_DETALLE_DESPACHO_Cantidad", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_DETALLE_DESPACHO_DESPACHO_DispatchId",
                        column: x => x.DispatchId,
                        principalTable: "DESPACHO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DETALLE_DESPACHO_PRODUCTO_ProductId",
                        column: x => x.ProductId,
                        principalTable: "PRODUCTO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DESPACHO_UserId",
                table: "DESPACHO",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DETALLE_DESPACHO_DispatchId_ProductId",
                table: "DETALLE_DESPACHO",
                columns: new[] { "DispatchId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DETALLE_DESPACHO_ProductId",
                table: "DETALLE_DESPACHO",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_MOVIMIENTO_INVENTARIO_ProductId",
                table: "MOVIMIENTO_INVENTARIO",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_MOVIMIENTO_INVENTARIO_UserId",
                table: "MOVIMIENTO_INVENTARIO",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DETALLE_DESPACHO");

            migrationBuilder.DropTable(
                name: "MOVIMIENTO_INVENTARIO");

            migrationBuilder.DropTable(
                name: "DESPACHO");
        }
    }
}
