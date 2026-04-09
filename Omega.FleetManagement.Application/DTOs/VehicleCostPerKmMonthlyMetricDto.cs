namespace Omega.FleetManagement.Application.DTOs
{
    public record VehicleCostPerKmMonthlyMetricDto(
        int Month,
        decimal TotalKm,
        decimal TotalExpense,
        decimal CostPerKm,
        List<VehicleCostPerKmExpenseTypeMetricDto> ExpenseTypes
    );
}
