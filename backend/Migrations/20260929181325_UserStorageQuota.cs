using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conectando.Api.Migrations
{
    /// <inheritdoc />
    public partial class UserStorageQuota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileImagePublicId",
                table: "users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProfileImageSizeBytes",
                table: "users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SizeBytes",
                table: "post_media",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            // Las filas que ya existen quedan en 0 porque su peso nunca se
            // guardó: no hay forma de recuperarlo sin ir a preguntarle a
            // Cloudinary archivo por archivo. Mientras haya datos reales hay
            // que backfillear contra la API de Cloudinary; recién ahí la cuenta
            // empieza a ser exacta.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileImagePublicId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ProfileImageSizeBytes",
                table: "users");

            migrationBuilder.DropColumn(
                name: "SizeBytes",
                table: "post_media");
        }
    }
}
