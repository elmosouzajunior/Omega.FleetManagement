namespace Omega.FleetManagement.Application.DTOs
{
    public record VehicleResponse(
        Guid Id,
        string LicensePlate,
        string Manufacturer,
        string Color,
        string? DriverName,
        Guid? DriverId,
        bool IsActive);    
}
