using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DShop2024.Migrations
{
    /// <inheritdoc />
    public partial class addDescriptPromotion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Promotion",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Promotion");
        }
    }
}
