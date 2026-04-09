namespace Omega.FleetManagement.Application.DTOs
{
    public record VehicleCostPerKmReportItemDto(
        Guid VehicleId,
        string LicensePlate,
        string Manufacturer,
        bool IsActive,
        List<VehicleCostPerKmMonthlyMetricDto> Months,
        decimal AnnualAverageCostPerKm
    );
}
