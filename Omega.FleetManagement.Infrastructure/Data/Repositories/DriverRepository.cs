using Microsoft.EntityFrameworkCore;
using Omega.FleetManagement.Domain.Entities;
using Omega.FleetManagement.Domain.Interfaces;
using Omega.FleetManagement.Infrastructure.Data.Context;

namespace Omega.FleetManagement.Infrastructure.Data.Repositories
{
    public class DriverRepository : IDriverRepository
    {
        private readonly FleetContext _context;
        public DriverRepository(FleetContext context)
        {
            _context = context;
        }

        public async Task<Driver?> GetByIdAsync(Guid driverId)
        {
            return await _context.Drivers
                .Include(d => d.Commissions)
                .FirstOrDefaultAsync(d => d.Id == driverId);
        }

        public async Task<Driver?> GetByIdAsync(Guid driverId, Guid companyId)
        {
            return await _context.Drivers
                .Include(d => d.Commissions)
                .FirstOrDefaultAsync(d => d.Id == driverId && d.CompanyId == companyId);
        }

        public async Task<Driver?> GetByCpfAsync(string cpf)
        {
            return await _context.Drivers.FirstOrDefaultAsync(d => d.Cpf == cpf);
        }

        public async Task<bool> ExistsByCpfAndCompanyIdAsync(string cpf, Guid companyId)
        {
            return await _context.Drivers.AnyAsync(d => d.Cpf == cpf && d.CompanyId == companyId);
        }

        public async Task AddAsync(Driver driver)
        {
            await _context.Drivers.AddAsync(driver);
        }

        public async Task<List<Driver>> GetByCompanyIdAsync(Guid companyId)
        {
            return await _context.Drivers
                .Include(d => d.Commissions)
                .Where(d => d.CompanyId == companyId)
                .ToListAsync();
        }

        public Task UpdateAsync(Driver driver)
        {
            _context.Drivers.Update(driver);
            return Task.CompletedTask;
        }
    }
}
