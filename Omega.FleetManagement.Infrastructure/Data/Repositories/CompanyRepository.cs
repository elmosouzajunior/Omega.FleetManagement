using Microsoft.EntityFrameworkCore;
using Omega.FleetManagement.Domain.Entities;
using Omega.FleetManagement.Domain.Interfaces;
using Omega.FleetManagement.Infrastructure.Data.Context;

namespace Omega.FleetManagement.Infrastructure.Data.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly FleetContext _context;
        public CompanyRepository(FleetContext context)
        {
            _context = context;
        }

        public IQueryable<Company> GetAllQueryable() => _context.Companies.AsNoTracking();

        public async Task<Company?> GetByIdAsync(Guid id)
        {
            return await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
        }

        public Task UpdateAsync(Company company)
        {
            _context.Companies.Update(company);
            return Task.CompletedTask;
        }
    }
}
