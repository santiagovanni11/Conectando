using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conectando.Api.Migrations
{
    /// <inheritdoc />
    public partial class PostSaves : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_post_saves_posts_PostId1",
                table: "post_saves");

            migrationBuilder.DropForeignKey(
                name: "FK_post_saves_users_UserId1",
                table: "post_saves");

            migrationBuilder.DropPrimaryKey(
                name: "PK_post_saves",
                table: "post_saves");

            migrationBuilder.DropIndex(
                name: "IX_post_saves_CreatedAt",
                table: "post_saves");

            migrationBuilder.DropIndex(
                name: "IX_post_saves_PostId1",
                table: "post_saves");

            migrationBuilder.DropIndex(
                name: "IX_post_saves_UserId1",
                table: "post_saves");

            migrationBuilder.DropColumn(
                name: "PostId1",
                table: "post_saves");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "post_saves");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "post_saves",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_post_saves",
                table: "post_saves",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_post_saves_UserId_CreatedAt",
                table: "post_saves",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_post_saves_UserId_PostId",
                table: "post_saves",
                columns: new[] { "UserId", "PostId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_post_saves",
                table: "post_saves");

            migrationBuilder.DropIndex(
                name: "IX_post_saves_UserId_CreatedAt",
                table: "post_saves");

            migrationBuilder.DropIndex(
                name: "IX_post_saves_UserId_PostId",
                table: "post_saves");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "post_saves");

            migrationBuilder.AddColumn<Guid>(
                name: "PostId1",
                table: "post_saves",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "post_saves",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_post_saves",
                table: "post_saves",
                columns: new[] { "UserId", "PostId" });

            migrationBuilder.CreateIndex(
                name: "IX_post_saves_CreatedAt",
                table: "post_saves",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_post_saves_PostId1",
                table: "post_saves",
                column: "PostId1");

            migrationBuilder.CreateIndex(
                name: "IX_post_saves_UserId1",
                table: "post_saves",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_post_saves_posts_PostId1",
                table: "post_saves",
                column: "PostId1",
                principalTable: "posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_post_saves_users_UserId1",
                table: "post_saves",
                column: "UserId1",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
