using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DShop2024.Migrations
{
    /// <inheritdoc />
    public partial class addUpdateByReturn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "Return",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdateUserId",
                table: "Return",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Return_UpdateUserId",
                table: "Return",
                column: "UpdateUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Return_AspNetUsers_UpdateUserId",
                table: "Return",
                column: "UpdateUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Return_AspNetUsers_UpdateUserId",
                table: "Return");

            migrationBuilder.DropIndex(
                name: "IX_Return_UpdateUserId",
                table: "Return");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "Return");

            migrationBuilder.DropColumn(
                name: "UpdateUserId",
                table: "Return");
        }
    }
}
