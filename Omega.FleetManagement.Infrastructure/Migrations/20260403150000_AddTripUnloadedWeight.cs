using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Omega.FleetManagement.Infrastructure.Data.Context;

#nullable disable

namespace Omega.FleetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(FleetContext))]
    [Migration("20260403150000_AddTripUnloadedWeight")]
    public partial class AddTripUnloadedWeight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "UnloadedWeightTons",
                table: "trips",
                type: "numeric(18,3)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnloadedWeightTons",
                table: "trips");
        }
    }
}
