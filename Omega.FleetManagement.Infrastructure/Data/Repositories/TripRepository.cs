using Microsoft.EntityFrameworkCore;
using Omega.FleetManagement.Domain.Entities;
using Omega.FleetManagement.Domain.Enums;
using Omega.FleetManagement.Domain.Interfaces;
using Omega.FleetManagement.Infrastructure.Data.Context;

namespace Omega.FleetManagement.Infrastructure.Data.Repositories
{
    public class TripRepository : ITripRepository
    {
        private readonly FleetContext _context;

        public TripRepository(FleetContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Trip trip)
        {
            await _context.Trips.AddAsync(trip);
        }

        public Task UpdateAsync(Trip trip)
        {
            _context.Trips.Update(trip);
            return Task.CompletedTask;
        }

        public async Task<Trip?> GetByIdAsync(Guid id, Guid companyId)
        {
            return await _context.Trips
                .Include(t => t.Driver)
                .Include(t => t.Vehicle)
                .Include(t => t.Expenses)
                    .ThenInclude(e => e.ExpenseType)
                .FirstOrDefaultAsync(t => t.Id == id && t.CompanyId == companyId);
        }

        public async Task<IEnumerable<Trip>> GetByCompanyIdAsync(Guid companyId)
        {
            return await _context.Trips
                .Include(t => t.Driver)
                .Include(t => t.Vehicle)
                .Where(t => t.CompanyId == companyId)                
                .OrderByDescending(t => t.LoadingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Trip>> GetActiveTripsByDriverAsync(Guid driverId)
        {
            return await _context.Trips
                .AsNoTracking()
                .Where(t => t.DriverId == driverId && t.Status == TripStatus.Open)
                .ToListAsync();
        }

        public async Task<bool> HasOpenTripAsync(Guid driverId, Guid? tripIdToIgnore = null)
        {
            return await _context.Trips.AnyAsync(t =>
                t.DriverId == driverId &&
                t.Status == TripStatus.Open &&
                (!tripIdToIgnore.HasValue || t.Id != tripIdToIgnore.Value));
        }
    }
}
