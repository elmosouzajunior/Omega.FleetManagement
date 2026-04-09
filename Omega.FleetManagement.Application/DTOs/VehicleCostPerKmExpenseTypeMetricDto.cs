namespace Omega.FleetManagement.Application.DTOs
{
    public record VehicleCostPerKmExpenseTypeMetricDto(
        string ExpenseTypeName,
        int CostCategory,
        decimal TotalExpense,
        decimal CostPerKm
    );
}
