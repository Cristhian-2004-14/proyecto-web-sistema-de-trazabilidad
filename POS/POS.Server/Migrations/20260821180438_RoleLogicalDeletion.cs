using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Server.Migrations
{
    /// <inheritdoc />
    public partial class RoleLogicalDeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ROL",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ROL");
        }
    }
}
