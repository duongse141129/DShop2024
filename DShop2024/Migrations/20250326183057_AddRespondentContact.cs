using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DShop2024.Migrations
{
    /// <inheritdoc />
    public partial class AddRespondentContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RespondentId",
                table: "Contact",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contact_RespondentId",
                table: "Contact",
                column: "RespondentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contact_AspNetUsers_RespondentId",
                table: "Contact",
                column: "RespondentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contact_AspNetUsers_RespondentId",
                table: "Contact");

            migrationBuilder.DropIndex(
                name: "IX_Contact_RespondentId",
                table: "Contact");

            migrationBuilder.DropColumn(
                name: "RespondentId",
                table: "Contact");
        }
    }
}
