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
            string unloadingLocation,
            DateTime loadingDate,
            decimal startKm,
            decimal freightValue,
            string? attachmentPath
        );
        Task<Trip> FinishTripAsync(Guid tripId, Guid companyId, DateTime unloadingDate, string? unloadingLocation, decimal finishKm);
        Task<Trip> ReopenTripAsync(Guid tripId, Guid companyId);
        Task<Trip> CancelTripAsync(Guid tripId, Guid companyId);
    }
}
