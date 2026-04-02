using Microsoft.AspNetCore.Http;

namespace Omega.FleetManagement.Application.DTOs
{
    public record OpenTripRequest(
        Guid DriverId,
        Guid VehicleId,
        string LoadingLocation,
        string UnloadingLocation,
        DateTime LoadingDate,
        decimal StartKm,
        decimal TonValue,
        decimal LoadedWeightTons,
        decimal FreightValue,
        IFormFile? FreightAttachment
    );
}
