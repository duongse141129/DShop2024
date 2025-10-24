using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DShop2024.Migrations
{
    /// <inheritdoc />
    public partial class removeFieldUpdateBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Post_AspNetUsers_UserIdUpdate",
                table: "Post");

            migrationBuilder.DropIndex(
                name: "IX_Post_UserIdUpdate",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "UserIdUpdate",
                table: "Post");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserIdUpdate",
                table: "Post",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Post_UserIdUpdate",
                table: "Post",
                column: "UserIdUpdate");

            migrationBuilder.AddForeignKey(
                name: "FK_Post_AspNetUsers_UserIdUpdate",
                table: "Post",
                column: "UserIdUpdate",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
