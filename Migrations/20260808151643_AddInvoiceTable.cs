using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KejaHUnt_PropertiesAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UnitId = table.Column<long>(type: "bigint", nullable: false),
                    PropertyId = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    UnitPaymentsId = table.Column<long>(type: "bigint", nullable: false),
                    PeriodMonth = table.Column<int>(type: "integer", nullable: false),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    RentAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    WaterBillAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_UnitPayments_UnitPaymentsId",
                        column: x => x.UnitPaymentsId,
                        principalTable: "UnitPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PropertyId",
                table: "Invoices",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_UnitId",
                table: "Invoices",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_UnitPaymentsId",
                table: "Invoices",
                column: "UnitPaymentsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Invoices");
        }
    }
}
