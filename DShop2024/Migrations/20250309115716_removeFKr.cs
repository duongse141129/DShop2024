using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DShop2024.Migrations
{
    /// <inheritdoc />
    public partial class removeFKr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rating_Product_ProductId",
                table: "Rating");

            migrationBuilder.DropIndex(
                name: "IX_Rating_ProductId",
                table: "Rating");

            migrationBuilder.AddColumn<int>(
                name: "RatingId",
                table: "Product",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product_RatingId",
                table: "Product",
                column: "RatingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Rating_RatingId",
                table: "Product",
                column: "RatingId",
                principalTable: "Rating",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_Rating_RatingId",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_RatingId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "RatingId",
                table: "Product");

            migrationBuilder.CreateIndex(
                name: "IX_Rating_ProductId",
                table: "Rating",
                column: "ProductId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Rating_Product_ProductId",
                table: "Rating",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
