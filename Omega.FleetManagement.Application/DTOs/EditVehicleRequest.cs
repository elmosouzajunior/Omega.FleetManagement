namespace Omega.FleetManagement.Application.DTOs
{
    public record EditVehicleRequest(
        Guid Id,
        string LicensePlate,
        string Manufacturer,
        string? Color,
        bool IsActive);    
}
