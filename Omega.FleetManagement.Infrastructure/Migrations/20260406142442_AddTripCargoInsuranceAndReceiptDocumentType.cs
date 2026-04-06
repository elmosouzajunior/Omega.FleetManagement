using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Omega.FleetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTripCargoInsuranceAndReceiptDocumentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CargoInsuranceValue",
                table: "trips",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReceiptDocumentTypeId",
                table: "trips",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptDocumentTypeName",
                table: "trips",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "receipt_document_types",
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
                    table.PrimaryKey("PK_receipt_document_types", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trips_ReceiptDocumentTypeId",
                table: "trips",
                column: "ReceiptDocumentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_trips_receipt_document_types_ReceiptDocumentTypeId",
                table: "trips",
                column: "ReceiptDocumentTypeId",
                principalTable: "receipt_document_types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_trips_receipt_document_types_ReceiptDocumentTypeId",
                table: "trips");

            migrationBuilder.DropTable(
                name: "receipt_document_types");

            migrationBuilder.DropIndex(
                name: "IX_trips_ReceiptDocumentTypeId",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "CargoInsuranceValue",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "ReceiptDocumentTypeId",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "ReceiptDocumentTypeName",
                table: "trips");
        }
    }
}
