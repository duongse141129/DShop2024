using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DShop2024.Migrations
{
    /// <inheritdoc />
    public partial class DeliveryByProvince : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Shipping");

            migrationBuilder.DropColumn(
                name: "District",
                table: "Shipping");

            migrationBuilder.RenameColumn(
                name: "Ward",
                table: "Shipping",
                newName: "Province");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Province",
                table: "Shipping",
                newName: "Ward");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Shipping",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "Shipping",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
