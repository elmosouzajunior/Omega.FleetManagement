using Microsoft.EntityFrameworkCore;
using Omega.FleetManagement.Domain.Entities;
using Omega.FleetManagement.Domain.Interfaces;
using Omega.FleetManagement.Infrastructure.Data.Context;

namespace Omega.FleetManagement.Infrastructure.Data.Repositories
{
    public class CompanyAdminRepository : ICompanyAdminRepository
    {
        private readonly FleetContext _context;

        public CompanyAdminRepository(FleetContext context)
        {
            _context = context;
        }

        public IQueryable<CompanyAdmin> GetAllQueryable() => _context.CompanyAdmins.AsNoTracking();
    }
}
