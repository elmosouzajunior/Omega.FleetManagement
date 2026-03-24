namespace Omega.FleetManagement.Application.DTOs
{
    public record VehicleCostPerKmReportItemDto(
        Guid VehicleId,
        string LicensePlate,
        string Manufacturer,
        decimal TotalKm,
        decimal TotalExpense,
        decimal CostPerKm
    );
}
