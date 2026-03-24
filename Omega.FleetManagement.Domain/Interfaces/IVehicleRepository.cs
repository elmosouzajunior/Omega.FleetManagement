using Omega.FleetManagement.Domain.Entities;

namespace Omega.FleetManagement.Domain.Interfaces
{
    public interface IVehicleRepository
    {
        Task<IEnumerable<Vehicle>> GetAllAsync();
        Task<List<Vehicle>> GetByCompanyIdAsync(Guid companyId);
        Task<Vehicle?> GetByIdAsync(Guid vehicleId);
        Task<Vehicle?> GetByIdAsync(Guid vehicleId, Guid companyId);
        Task<bool> ExistsByLicensePlateAndCompanyIdAsync(string licensePlate, Guid companyId);
        Task AddAsync(Vehicle vehicle);
        Task UpdateAsync(Vehicle vehicle);
        Task<bool> IsDriverAllocatedAsync(Guid driverId, Guid? vehicleIdToIgnore = null);
        Task<bool> IsDriverAllocatedAsync(Guid driverId, Guid companyId, Guid? vehicleIdToIgnore = null);
    }
}
