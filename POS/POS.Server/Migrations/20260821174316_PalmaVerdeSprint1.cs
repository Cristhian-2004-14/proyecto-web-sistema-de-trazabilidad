using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Server.Migrations
{
    /// <inheritdoc />
    public partial class PalmaVerdeSprint1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CashClosings_Users_UserId",
                table: "CashClosings");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Users_UserId",
                table: "Sales");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Users_UserId",
                table: "StockMovements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "USUARIO");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "USUARIO",
                newName: "RoleId");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "USUARIO",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "USUARIO",
                type: "nvarchar(225)",
                maxLength: 225,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "USUARIO",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "USUARIO",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "USUARIO",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_USUARIO",
                table: "USUARIO",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "MATERIA_PRIMA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CurrentStock = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumStock = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MATERIA_PRIMA", x => x.Id);
                    table.CheckConstraint("CK_MATERIA_PRIMA_StockActual", "[CurrentStock] >= 0");
                    table.CheckConstraint("CK_MATERIA_PRIMA_StockMinimo", "[MinimumStock] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "PROVEEDOR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROVEEDOR", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ROL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROL", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_USUARIO_Email",
                table: "USUARIO",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USUARIO_RoleId",
                table: "USUARIO",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_USUARIO_Username",
                table: "USUARIO",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ROL_Name",
                table: "ROL",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CashClosings_USUARIO_UserId",
                table: "CashClosings",
                column: "UserId",
                principalTable: "USUARIO",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_USUARIO_UserId",
                table: "Sales",
                column: "UserId",
                principalTable: "USUARIO",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_USUARIO_UserId",
                table: "StockMovements",
                column: "UserId",
                principalTable: "USUARIO",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_USUARIO_ROL_RoleId",
                table: "USUARIO",
                column: "RoleId",
                principalTable: "ROL",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CashClosings_USUARIO_UserId",
                table: "CashClosings");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_USUARIO_UserId",
                table: "Sales");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_USUARIO_UserId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_USUARIO_ROL_RoleId",
                table: "USUARIO");

            migrationBuilder.DropTable(
                name: "MATERIA_PRIMA");

            migrationBuilder.DropTable(
                name: "PROVEEDOR");

            migrationBuilder.DropTable(
                name: "ROL");

            migrationBuilder.DropPrimaryKey(
                name: "PK_USUARIO",
                table: "USUARIO");

            migrationBuilder.DropIndex(
                name: "IX_USUARIO_Email",
                table: "USUARIO");

            migrationBuilder.DropIndex(
                name: "IX_USUARIO_RoleId",
                table: "USUARIO");

            migrationBuilder.DropIndex(
                name: "IX_USUARIO_Username",
                table: "USUARIO");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "USUARIO");

            migrationBuilder.RenameTable(
                name: "USUARIO",
                newName: "Users");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "Users",
                newName: "Role");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(225)",
                oldMaxLength: 225);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CashClosings_Users_UserId",
                table: "CashClosings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Users_UserId",
                table: "Sales",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Users_UserId",
                table: "StockMovements",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
