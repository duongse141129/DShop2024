using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DShop2024.Migrations
{
    /// <inheritdoc />
    public partial class AddMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Profit",
                table: "Statistical");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Statistical");

            migrationBuilder.RenameColumn(
                name: "Revenue",
                table: "Statistical",
                newName: "revenue");

            migrationBuilder.RenameColumn(
                name: "Sold",
                table: "Statistical",
                newName: "orders");

            migrationBuilder.RenameColumn(
                name: "DateCreate",
                table: "Statistical",
                newName: "date");

            migrationBuilder.AlterColumn<decimal>(
                name: "revenue",
                table: "Statistical",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "Message",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContentMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    To = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Message", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Message_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Message_UserId",
                table: "Message",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Message");

            migrationBuilder.RenameColumn(
                name: "revenue",
                table: "Statistical",
                newName: "Revenue");

            migrationBuilder.RenameColumn(
                name: "orders",
                table: "Statistical",
                newName: "Sold");

            migrationBuilder.RenameColumn(
                name: "date",
                table: "Statistical",
                newName: "DateCreate");

            migrationBuilder.AlterColumn<int>(
                name: "Revenue",
                table: "Statistical",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<int>(
                name: "Profit",
                table: "Statistical",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Statistical",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
