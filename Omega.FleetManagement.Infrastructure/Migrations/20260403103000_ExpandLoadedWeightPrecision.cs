using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Omega.FleetManagement.Infrastructure.Data.Context;

#nullable disable

namespace Omega.FleetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(FleetContext))]
    [Migration("20260403103000_ExpandLoadedWeightPrecision")]
    public partial class ExpandLoadedWeightPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "LoadedWeightTons",
                table: "trips",
                type: "numeric(18,3)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldDefaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "LoadedWeightTons",
                table: "trips",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,3)",
                oldDefaultValue: 0m);
        }
    }
}
