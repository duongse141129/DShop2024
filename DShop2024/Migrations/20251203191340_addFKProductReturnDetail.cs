using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DShop2024.Migrations
{
    /// <inheritdoc />
    public partial class addFKProductReturnDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ReturnDetail_ProductId",
                table: "ReturnDetail",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnDetail_Product_ProductId",
                table: "ReturnDetail",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReturnDetail_Product_ProductId",
                table: "ReturnDetail");

            migrationBuilder.DropIndex(
                name: "IX_ReturnDetail_ProductId",
                table: "ReturnDetail");
        }
    }
}
