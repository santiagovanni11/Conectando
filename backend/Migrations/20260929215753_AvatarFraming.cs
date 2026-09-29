using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conectando.Api.Migrations
{
    /// <inheritdoc />
    public partial class AvatarFraming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProfileImageOffsetX",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProfileImageOffsetY",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "ProfileImageZoom",
                table: "users",
                type: "double precision",
                nullable: false,
                defaultValue: 1.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileImageOffsetX",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ProfileImageOffsetY",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ProfileImageZoom",
                table: "users");
        }
    }
}
