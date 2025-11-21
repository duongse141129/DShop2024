using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DShop2024.Migrations
{
    /// <inheritdoc />
    public partial class addReplyRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReplyMessage",
                table: "Rating",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserIdReply",
                table: "Rating",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rating_UserIdReply",
                table: "Rating",
                column: "UserIdReply");

            migrationBuilder.AddForeignKey(
                name: "FK_Rating_AspNetUsers_UserIdReply",
                table: "Rating",
                column: "UserIdReply",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rating_AspNetUsers_UserIdReply",
                table: "Rating");

            migrationBuilder.DropIndex(
                name: "IX_Rating_UserIdReply",
                table: "Rating");

            migrationBuilder.DropColumn(
                name: "ReplyMessage",
                table: "Rating");

            migrationBuilder.DropColumn(
                name: "UserIdReply",
                table: "Rating");
        }
    }
}
