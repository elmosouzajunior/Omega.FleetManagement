namespace Omega.FleetManagement.Application.DTOs
{
    public record CreateVehicleRequest(string LicensePlate, string Manufacturer, string Color, string Model, int Year, decimal CurrentKm);
}