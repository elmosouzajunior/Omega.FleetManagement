using Omega.FleetManagement.Application.DTOs;

namespace Omega.FleetManagement.Application.Interfaces
{
    public interface IReportAppService
    {
        Task<VehicleCostPerKmReportDto> GetVehicleCostPerKmAsync(Guid companyId, int? year = null);
    }
}
