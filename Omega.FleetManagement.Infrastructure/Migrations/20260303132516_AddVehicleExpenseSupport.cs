using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Omega.FleetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleExpenseSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "TripId",
                table: "expenses",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "VehicleId",
                table: "expenses",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_expenses_VehicleId",
                table: "expenses",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_expenses_vehicles_VehicleId",
                table: "expenses",
                column: "VehicleId",
                principalTable: "vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_expenses_vehicles_VehicleId",
                table: "expenses");

            migrationBuilder.DropIndex(
                name: "IX_expenses_VehicleId",
                table: "expenses");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "expenses");

            migrationBuilder.AlterColumn<Guid>(
                name: "TripId",
                table: "expenses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
