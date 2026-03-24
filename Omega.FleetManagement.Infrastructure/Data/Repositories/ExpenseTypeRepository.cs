using Microsoft.EntityFrameworkCore;
using Omega.FleetManagement.Domain.Entities;
using Omega.FleetManagement.Domain.Interfaces;
using Omega.FleetManagement.Infrastructure.Data.Context;

namespace Omega.FleetManagement.Infrastructure.Data.Repositories
{
    public class ExpenseTypeRepository : IExpenseTypeRepository
    {
        private readonly FleetContext _context;

        public ExpenseTypeRepository(FleetContext context)
        {
            _context = context;
        }

        public async Task<ExpenseType?> GetByIdAsync(Guid id, bool includeInactive = false)
        {
            var query = _context.ExpenseTypes.Where(e => e.Id == id);
            if (!includeInactive)
                query = query.Where(e => e.IsActive);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<ExpenseType>> GetByCompanyIdAsync(Guid companyId, bool includeInactive = false)
        {
            var query = _context.ExpenseTypes.Where(e => e.CompanyId == companyId);

            if (!includeInactive)
                query = query.Where(e => e.IsActive);

            return await query
                .AsNoTracking()
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        public async Task<List<ExpenseType>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.ExpenseTypes.AsQueryable();

            if (!includeInactive)
                query = query.Where(e => e.IsActive);

            return await query
                .AsNoTracking()
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        public async Task AddAsync(ExpenseType expenseType)
        {
            await _context.ExpenseTypes.AddAsync(expenseType);
        }
    }
}
