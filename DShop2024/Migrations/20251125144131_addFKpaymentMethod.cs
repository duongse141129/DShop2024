using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DShop2024.Migrations
{
    /// <inheritdoc />
    public partial class addFKpaymentMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_Order_PaymentId",
                table: "Order",
                column: "PaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Payment_PaymentId",
                table: "Order",
                column: "PaymentId",
                principalTable: "Payment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Payment_PaymentId",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_Order_PaymentId",
                table: "Order");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Order",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
