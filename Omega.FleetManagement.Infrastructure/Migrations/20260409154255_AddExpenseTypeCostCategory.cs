using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Omega.FleetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseTypeCostCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CostCategory",
                table: "expense_types",
                type: "int",
                nullable: false,
                defaultValue: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostCategory",
                table: "expense_types");
        }
    }
}
