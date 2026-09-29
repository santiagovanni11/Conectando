using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conectando.Api.Migrations
{
    /// <inheritdoc />
    public partial class MessageEditAndDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EditedAt",
                table: "messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "conversation_members",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditedAt",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "conversation_members");
        }
    }
}
