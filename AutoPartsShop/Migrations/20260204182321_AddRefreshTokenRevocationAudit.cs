using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoPartsShop.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenRevocationAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedAt",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RevokedByUserId",
                table: "RefreshTokens",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_RevokedByUserId",
                table: "RefreshTokens",
                column: "RevokedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Users_RevokedByUserId",
                table: "RefreshTokens",
                column: "RevokedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Users_RevokedByUserId",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_RevokedByUserId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "RevokedAt",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "RevokedByUserId",
                table: "RefreshTokens");
        }
    }
}
