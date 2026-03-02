using Omega.FleetManagement.Domain.Interfaces;
using Omega.FleetManagement.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Omega.FleetManagement.Domain.Enums;

namespace Omega.FleetManagement.Infrastructure.Data.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly FleetContext _context;
        public DashboardRepository(FleetContext context)
        {
            _context = context;
        }

        public async Task<int> GetActiveDriversCountAsync(Guid companyId)
        {
            return await _context.Drivers.CountAsync(d => d.CompanyId == companyId && d.IsActive);
        }

        public async Task<int> GetActiveVehiclesCountAsync(Guid companyId)
        {
            return await _context.Vehicles.CountAsync(v => v.CompanyId == companyId && v.IsActive);
        }

        public async Task<int> GetOpenTripsCountAsync(Guid companyId)
        {
            return await _context.Trips.CountAsync(t => t.CompanyId == companyId && t.Status == TripStatus.Open);
        }
    }
}
