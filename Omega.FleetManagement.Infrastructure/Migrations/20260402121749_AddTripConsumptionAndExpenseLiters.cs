using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Omega.FleetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTripConsumptionAndExpenseLiters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ArlaKmPerLiter",
                table: "trips",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DieselKmPerLiter",
                table: "trips",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Liters",
                table: "expenses",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArlaKmPerLiter",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "DieselKmPerLiter",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "Liters",
                table: "expenses");
        }
    }
}
