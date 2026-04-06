using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Omega.FleetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExpensePricePerLiter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PricePerLiter",
                table: "expenses",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PricePerLiter",
                table: "expenses");
        }
    }
}
