using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DShop2024.Migrations
{
    /// <inheritdoc />
    public partial class editFieldsRedemptionCoupon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "status",
                table: "OrderCoupons",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "CouponRedemption",
                newName: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "OrderCoupons",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "CouponRedemption",
                newName: "status");
        }
    }
}
