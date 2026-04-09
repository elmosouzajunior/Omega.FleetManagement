namespace Omega.FleetManagement.Application.DTOs
{
    public record VehicleProfitReportItemDto(
        Guid VehicleId,
        string LicensePlate,
        string Manufacturer,
        bool IsActive,
        decimal Revenue,
        decimal TripExpense,
        decimal DirectExpense,
        decimal CommissionExpense,
        decimal CargoInsuranceExpense,
        decimal TotalExpense,
        decimal NetProfit
    );
}
