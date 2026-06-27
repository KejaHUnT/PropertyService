using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KejaHUnt_PropertiesAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddShowPriceToProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowPrice",
                table: "Units");

            migrationBuilder.AddColumn<bool>(
                name: "ShowPrice",
                table: "Properties",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowPrice",
                table: "Properties");

            migrationBuilder.AddColumn<bool>(
                name: "ShowPrice",
                table: "Units",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }
    }
}
