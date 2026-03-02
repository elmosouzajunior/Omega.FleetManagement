using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Omega.FleetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpensesType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_expenses_ExpenseTypes_ExpenseTypeId",
                table: "expenses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExpenseTypes",
                table: "ExpenseTypes");

            migrationBuilder.RenameTable(
                name: "ExpenseTypes",
                newName: "expense_types");

            migrationBuilder.AddPrimaryKey(
                name: "PK_expense_types",
                table: "expense_types",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_expenses_expense_types_ExpenseTypeId",
                table: "expenses",
                column: "ExpenseTypeId",
                principalTable: "expense_types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_expenses_expense_types_ExpenseTypeId",
                table: "expenses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_expense_types",
                table: "expense_types");

            migrationBuilder.RenameTable(
                name: "expense_types",
                newName: "ExpenseTypes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExpenseTypes",
                table: "ExpenseTypes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_expenses_ExpenseTypes_ExpenseTypeId",
                table: "expenses",
                column: "ExpenseTypeId",
                principalTable: "ExpenseTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
