using Omega.FleetManagement.Application.DTOs;

namespace Omega.FleetManagement.Application.Interfaces
{
    public interface IVehicleAppService
    {
        Task<List<VehicleResponse>> GetVehiclesByCompanyIdAsync(Guid companyId);
        Task CreateVehicleAsync(CreateVehicleRequest request, Guid companyId);
        Task<bool> AssignDriverAsync(Guid vehicleId, Guid driverId);
        Task<bool> UpdateVehicleAsync(Guid driverId, EditVehicleRequest request);
    }
}
