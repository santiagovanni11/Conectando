using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conectando.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "blocks",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockedUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blocks", x => new { x.UserId, x.BlockedUserId });
                    table.CheckConstraint("CK_blocks_no_self", "\"UserId\" <> \"BlockedUserId\"");
                    table.ForeignKey(
                        name: "FK_blocks_users_BlockedUserId",
                        column: x => x.BlockedUserId,
                        principalTable: "users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_blocks_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "follows",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_follows", x => new { x.UserId, x.TargetUserId });
                    table.CheckConstraint("CK_follows_no_self", "\"UserId\" <> \"TargetUserId\"");
                    table.ForeignKey(
                        name: "FK_follows_users_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_follows_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "friend_requests",
                columns: table => new
                {
                    RequesterId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddresseeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_friend_requests", x => new { x.RequesterId, x.AddresseeId });
                    table.CheckConstraint("CK_friend_requests_no_self", "\"RequesterId\" <> \"AddresseeId\"");
                    table.ForeignKey(
                        name: "FK_friend_requests_users_AddresseeId",
                        column: x => x.AddresseeId,
                        principalTable: "users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_friend_requests_users_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "friendships",
                columns: table => new
                {
                    UserLowId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserHighId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_friendships", x => new { x.UserLowId, x.UserHighId });
                    table.CheckConstraint("CK_friendships_normalized", "\"UserLowId\" < \"UserHighId\"");
                    table.ForeignKey(
                        name: "FK_friendships_users_UserHighId",
                        column: x => x.UserHighId,
                        principalTable: "users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_friendships_users_UserLowId",
                        column: x => x.UserLowId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_blocks_BlockedUserId",
                table: "blocks",
                column: "BlockedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_follows_TargetUserId",
                table: "follows",
                column: "TargetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_friend_requests_AddresseeId",
                table: "friend_requests",
                column: "AddresseeId");

            migrationBuilder.CreateIndex(
                name: "IX_friendships_UserHighId",
                table: "friendships",
                column: "UserHighId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blocks");

            migrationBuilder.DropTable(
                name: "follows");

            migrationBuilder.DropTable(
                name: "friend_requests");

            migrationBuilder.DropTable(
                name: "friendships");
        }
    }
}
