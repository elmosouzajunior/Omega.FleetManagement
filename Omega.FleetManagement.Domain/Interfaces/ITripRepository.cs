using Omega.FleetManagement.Domain.Entities;

namespace Omega.FleetManagement.Domain.Interfaces
{
    public interface ITripRepository
    {     
        Task AddAsync(Trip trip);
        Task<Trip?> GetByIdAsync(Guid id, Guid companyId);
        Task<IEnumerable<Trip>> GetByCompanyIdAsync(Guid companyId);
        Task<IEnumerable<Trip>> GetActiveTripsByDriverAsync(Guid driverId);
        Task<bool> HasOpenTripAsync(Guid driverId);
    }
}
