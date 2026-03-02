using Microsoft.AspNetCore.Http;

namespace Omega.FleetManagement.Application.DTOs
{
    public record OpenTripRequest(
        Guid DriverId,
        Guid VehicleId,
        string LoadingLocation,
        DateTime LoadingDate,
        decimal StartKm,
        decimal FreightValue,
        IFormFile? FreightAttachment
    );
}