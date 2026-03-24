using Omega.FleetManagement.Application.DTOs;

namespace Omega.FleetManagement.Application.Interfaces
{
    public interface IDriverAppService
    {
        Task<bool> CreateDriverAsync(CreateDriverRequest request, Guid companyId);
        Task<List<DriverResponse>> GetDriversByCompanyIdAsync(Guid companyId);
        Task<bool> UpdateDriverAsync(Guid driverId, EditDriverRequest request, Guid companyId);
    }
}
