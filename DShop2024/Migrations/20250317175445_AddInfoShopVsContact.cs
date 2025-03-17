using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DShop2024.Migrations
{
    /// <inheritdoc />
    public partial class AddInfoShopVsContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Contact");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Contact");

            migrationBuilder.DropColumn(
                name: "LogoImg",
                table: "Contact");

            migrationBuilder.DropColumn(
                name: "Map",
                table: "Contact");

            migrationBuilder.RenameColumn(
                name: "ShopName",
                table: "Contact",
                newName: "Subject");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Contact",
                newName: "Message");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateSent",
                table: "Contact",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Contact",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Contact",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InformationShop",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShopName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Map = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogoImg = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InformationShop", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contact_UserId",
                table: "Contact",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contact_AspNetUsers_UserId",
                table: "Contact",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contact_AspNetUsers_UserId",
                table: "Contact");

            migrationBuilder.DropTable(
                name: "InformationShop");

            migrationBuilder.DropIndex(
                name: "IX_Contact_UserId",
                table: "Contact");

            migrationBuilder.DropColumn(
                name: "DateSent",
                table: "Contact");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Contact");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Contact");

            migrationBuilder.RenameColumn(
                name: "Subject",
                table: "Contact",
                newName: "ShopName");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "Contact",
                newName: "Phone");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Contact",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Contact",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LogoImg",
                table: "Contact",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Map",
                table: "Contact",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
