using Omega.FleetManagement.Domain.Entities;

namespace Omega.FleetManagement.Domain.Interfaces
{
    public interface IVehicleRepository
    {
        Task<IEnumerable<Vehicle>> GetAllAsync();
        Task<List<Vehicle>> GetByCompanyIdAsync(Guid companyId);
        Task<Vehicle?> GetByIdAsync(Guid vehicleId);
        Task<bool> ExistsByLicensePlateAndCompanyIdAsync(string licensePlate, Guid companyId);
        Task AddAsync(Vehicle vehicle);
        Task UpdateAsync(Vehicle vehicle);
        Task<bool> IsDriverAllocatedAsync(Guid driverId, Guid? vehicleIdToIgnore = null);
    }
}
