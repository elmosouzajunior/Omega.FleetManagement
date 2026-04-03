using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Omega.FleetManagement.Infrastructure.Data.Context;

#nullable disable

namespace Omega.FleetManagement.Infrastructure.Migrations
{
    [DbContext(typeof(FleetContext))]
    [Migration("20260403170000_AddDriverCommissions")]
    public partial class AddDriverCommissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "driver_commissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DriverId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_driver_commissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_driver_commissions_drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_driver_commissions_DriverId_Rate",
                table: "driver_commissions",
                columns: new[] { "DriverId", "Rate" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO driver_commissions (Id, DriverId, Rate)
                SELECT NEWID(), d.Id, d.CommissionRate
                FROM drivers d
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM driver_commissions dc
                    WHERE dc.DriverId = d.Id AND dc.Rate = d.CommissionRate
                )
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "driver_commissions");
        }
    }
}
