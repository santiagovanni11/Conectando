using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conectando.Api.Migrations
{
    /// <inheritdoc />
    public partial class AccountSecurityStampAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            // Sello único por usuario y no un Guid vacío compartido: el valor
            // viaja en el token, y que dos cuentas compartieran el mismo haría
            // que una sesión pudiera valer para la otra.
            migrationBuilder.Sql(
                """
                ALTER TABLE "users"
                ADD COLUMN "SecurityStamp" uuid NOT NULL DEFAULT gen_random_uuid();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "users");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "users");
        }
    }
}
