using Omega.FleetManagement.Domain.Entities;

namespace Omega.FleetManagement.Domain.Interfaces
{
    public interface ITripService
    {
        Task<Trip> OpenTripAsync(
            Guid companyId,
            Guid driverId,
            Guid vehicleId,
            string loadingLocation,
            DateTime loadingDate,
            decimal startKm,
            decimal freightValue,
            string attachmentPath
        );
    }
}
