using Microsoft.EntityFrameworkCore;
using Omega.FleetManagement.Domain.Entities;
using Omega.FleetManagement.Domain.Interfaces;
using Omega.FleetManagement.Infrastructure.Data.Context;

namespace Omega.FleetManagement.Infrastructure.Data.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly FleetContext _context;

        public ProductRepository(FleetContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(Guid id, bool includeInactive = false)
        {
            var query = _context.Products.Where(product => product.Id == id);

            if (!includeInactive)
                query = query.Where(product => product.IsActive);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<Product>> GetByCompanyIdAsync(Guid companyId, bool includeInactive = false)
        {
            var query = _context.Products.Where(product => product.CompanyId == companyId);

            if (!includeInactive)
                query = query.Where(product => product.IsActive);

            return await query
                .OrderBy(product => product.Name)
                .ToListAsync();
        }

        public async Task<List<Product>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.Products.AsQueryable();

            if (!includeInactive)
                query = query.Where(product => product.IsActive);

            return await query
                .OrderBy(product => product.Name)
                .ToListAsync();
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
        }
    }
}
