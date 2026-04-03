using Omega.FleetManagement.Domain.Entities;

namespace Omega.FleetManagement.Domain.Interfaces
{
    public interface ITripService
    {
        Task<Trip> OpenTripAsync(
            Guid companyId,
            Guid driverId,
            Guid vehicleId,
            decimal commissionPercent,
            string loadingLocation,
            string unloadingLocation,
            DateTime loadingDate,
            decimal startKm,
            decimal tonValue,
            decimal loadedWeightTons,
            decimal freightValue,
            string? attachmentPath
        );
        Task<Trip> UpdateTripOpeningAsync(
            Guid tripId,
            Guid companyId,
            Guid driverId,
            Guid vehicleId,
            string loadingLocation,
            string unloadingLocation,
            DateTime loadingDate,
            decimal startKm,
            decimal tonValue,
            decimal loadedWeightTons,
            decimal freightValue);
        Task<Trip> FinishTripAsync(Guid tripId, Guid companyId, DateTime unloadingDate, string? unloadingLocation, decimal finishKm, decimal unloadedWeightTons, decimal freightValue, decimal? dieselKmPerLiter, decimal? arlaKmPerLiter);
        Task<Trip> ReopenTripAsync(Guid tripId, Guid companyId);
        Task<Trip> CancelTripAsync(Guid tripId, Guid companyId);
    }
}
