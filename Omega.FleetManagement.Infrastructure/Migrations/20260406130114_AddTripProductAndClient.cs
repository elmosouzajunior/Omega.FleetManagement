using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Omega.FleetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTripProductAndClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientName",
                table: "trips",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "trips",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "trips",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trips_ProductId",
                table: "trips",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_trips_products_ProductId",
                table: "trips",
                column: "ProductId",
                principalTable: "products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_trips_products_ProductId",
                table: "trips");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropIndex(
                name: "IX_trips_ProductId",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "ClientName",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "trips");
        }
    }
}
