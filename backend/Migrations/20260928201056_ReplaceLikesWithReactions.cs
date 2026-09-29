using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conectando.Api.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceLikesWithReactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // El orden importa: primero se crea la tabla destino, después
            // se copia y recién al final se descarta la vieja. Así ningún
            // "me gusta" existente se pierde en la migración.
            migrationBuilder.CreateTable(
                name: "reactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Target = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reactions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_reactions_Target_TargetId_Type",
                table: "reactions",
                columns: new[] { "Target", "TargetId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_reactions_UserId_Target_TargetId",
                table: "reactions",
                columns: new[] { "UserId", "Target", "TargetId" },
                unique: true);

            // Los "me gusta" que ya existían pasan a ser reacciones de tipo
            // Like sobre posts. gen_random_uuid() da un id nuevo porque la
            // tabla nueva no reutiliza la clave compuesta del like.
            migrationBuilder.Sql("""
                INSERT INTO "reactions" ("Id", "UserId", "Target", "TargetId", "Type", "CreatedAt")
                SELECT gen_random_uuid(), "UserId", 'Post', "PostId", 'Like', "CreatedAt"
                FROM "likes";
                """);

            migrationBuilder.DropTable(
                name: "likes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reactions");

            migrationBuilder.CreateTable(
                name: "likes",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_likes", x => new { x.UserId, x.PostId });
                    table.ForeignKey(
                        name: "FK_likes_posts_PostId",
                        column: x => x.PostId,
                        principalTable: "posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_likes_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_likes_PostId_CreatedAt_UserId",
                table: "likes",
                columns: new[] { "PostId", "CreatedAt", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_likes_PostId_UserId",
                table: "likes",
                columns: new[] { "PostId", "UserId" });
        }
    }
}
