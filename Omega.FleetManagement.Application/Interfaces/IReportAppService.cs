using Omega.FleetManagement.Application.DTOs;

namespace Omega.FleetManagement.Application.Interfaces
{
    public interface IReportAppService
    {
        Task<List<VehicleCostPerKmReportItemDto>> GetVehicleCostPerKmAsync(Guid companyId);
    }
}
