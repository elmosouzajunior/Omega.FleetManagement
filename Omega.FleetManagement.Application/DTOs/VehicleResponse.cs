namespace Omega.FleetManagement.Application.DTOs
{
    public record VehicleResponse(
        Guid Id,
        string LicensePlate,
        string Manufacturer,
        string Color,
        decimal? LoadCapacityTons,
        string? DriverName,
        Guid? DriverId,
        bool IsActive);    
}
