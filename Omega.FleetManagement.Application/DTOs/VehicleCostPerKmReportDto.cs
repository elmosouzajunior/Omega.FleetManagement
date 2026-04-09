namespace Omega.FleetManagement.Application.DTOs
{
    public record VehicleCostPerKmReportDto(
        int Year,
        List<int> AvailableYears,
        List<VehicleCostPerKmReportItemDto> Items
    );
}
