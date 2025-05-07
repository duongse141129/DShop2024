using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DShop2024.Migrations
{
    /// <inheritdoc />
    public partial class addPlugin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PluginFacebook",
                table: "InformationShop",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PluginYoutube",
                table: "InformationShop",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PluginFacebook",
                table: "InformationShop");

            migrationBuilder.DropColumn(
                name: "PluginYoutube",
                table: "InformationShop");
        }
    }
}
