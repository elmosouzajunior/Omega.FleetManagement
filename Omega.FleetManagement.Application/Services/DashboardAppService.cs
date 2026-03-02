using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;
using Omega.FleetManagement.Domain.Interfaces;

namespace Omega.FleetManagement.Application.Services
{
    public class DashboardAppService : IDashboardAppService
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardAppService(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<AdminDashboardDto> GetAdminSummaryAsync(Guid companyId)
        {
            var activeDrivers = await _dashboardRepository.GetActiveDriversCountAsync(companyId);
            var activeVehicles = await _dashboardRepository.GetActiveVehiclesCountAsync(companyId);
            var openTrips = await _dashboardRepository.GetOpenTripsCountAsync(companyId);
            return new AdminDashboardDto
            {
                ActiveDrivers = activeDrivers,
                ActiveVehicles = activeVehicles,
                OpenTrips = openTrips
            };
        }
    }
}
