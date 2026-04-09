namespace Omega.FleetManagement.Application.DTOs
{
    public record VehicleCostPerKmExpenseTypeMetricDto(
        string ExpenseTypeName,
        decimal TotalExpense,
        decimal CostPerKm
    );
}
