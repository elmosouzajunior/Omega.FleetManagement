using Omega.FleetManagement.Application.DTOs;

namespace Omega.FleetManagement.Application.Interfaces
{
    public interface IDashboardAppService
    {
        Task<AdminDashboardDto> GetAdminSummaryAsync(Guid companyId);
    }
}
