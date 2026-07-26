using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KejaHUnt_PropertiesAPI.Migrations
{
    /// <inheritdoc />
    public partial class RenameUnitStatusAvailableToVacant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE ""Units"" SET ""Status"" = 'Vacant' WHERE ""Status"" = 'Available';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE ""Units"" SET ""Status"" = 'Available' WHERE ""Status"" = 'Vacant';");
        }
    }
}