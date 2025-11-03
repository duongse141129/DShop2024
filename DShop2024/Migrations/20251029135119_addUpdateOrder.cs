using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DShop2024.Migrations
{
    /// <inheritdoc />
    public partial class addUpdateOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateUpdate",
                table: "Order",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UserIdUpdate",
                table: "Order",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Order_UserIdUpdate",
                table: "Order",
                column: "UserIdUpdate");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_AspNetUsers_UserIdUpdate",
                table: "Order",
                column: "UserIdUpdate",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_AspNetUsers_UserIdUpdate",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_Order_UserIdUpdate",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "DateUpdate",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "UserIdUpdate",
                table: "Order");
        }
    }
}
