namespace Omega.FleetManagement.Application.DTOs
{
    public record VehicleProfitReportDto(
        int Year,
        List<int> AvailableYears,
        List<VehicleProfitReportItemDto> Items
    );
}
