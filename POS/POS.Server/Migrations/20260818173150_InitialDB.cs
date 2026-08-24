using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Server.Migrations;

/// <summary>
/// Migración duplicada conservada por compatibilidad con el historial del proyecto base.
/// La migración 20260819004804_InitialDB contiene el esquema inicial efectivo.
/// </summary>
public partial class InitialDBBase : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) { }
    protected override void Down(MigrationBuilder migrationBuilder) { }
}
