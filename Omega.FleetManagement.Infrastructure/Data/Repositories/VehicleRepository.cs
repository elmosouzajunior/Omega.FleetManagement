using Microsoft.EntityFrameworkCore;
using Omega.FleetManagement.Domain.Entities;
using Omega.FleetManagement.Domain.Interfaces;
using Omega.FleetManagement.Infrastructure.Data.Context;

namespace Omega.FleetManagement.Infrastructure.Data.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly FleetContext _context;

        public VehicleRepository(FleetContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Vehicle>> GetAllAsync()
        {
            return await _context.Vehicles.Where(v => v.IsActive).ToListAsync();
        }

        public async Task<List<Vehicle>> GetByCompanyIdAsync(Guid companyId)
        {
            return await _context.Vehicles
                .Include(v => v.Driver)
                .Where(v => v.CompanyId == companyId)
                .ToListAsync();
        }

        public async Task<Vehicle?> GetByIdAsync(Guid id)
        {
            return await _context.Vehicles.FindAsync(id);
        }        

        public async Task<bool> ExistsByLicensePlateAndCompanyIdAsync(string licensePlate, Guid companyId)
        {
            return await _context.Vehicles
                .AnyAsync(v => v.LicensePlate == licensePlate && v.CompanyId == companyId);
        }

        public async Task AddAsync(Vehicle vehicle)
        {
            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Vehicle vehicle)
        {
            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsDriverAllocatedAsync(Guid driverId, Guid? vehicleIdToIgnore = null)
        {
            return await _context.Vehicles
                .AnyAsync(v => v.DriverId == driverId && v.Id != vehicleIdToIgnore);
        }
    }
}
