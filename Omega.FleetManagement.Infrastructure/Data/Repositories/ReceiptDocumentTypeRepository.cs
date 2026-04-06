using Microsoft.EntityFrameworkCore;
using Omega.FleetManagement.Domain.Entities;
using Omega.FleetManagement.Domain.Interfaces;
using Omega.FleetManagement.Infrastructure.Data.Context;

namespace Omega.FleetManagement.Infrastructure.Data.Repositories
{
    public class ReceiptDocumentTypeRepository : IReceiptDocumentTypeRepository
    {
        private readonly FleetContext _context;

        public ReceiptDocumentTypeRepository(FleetContext context)
        {
            _context = context;
        }

        public async Task<ReceiptDocumentType?> GetByIdAsync(Guid id, bool includeInactive = false)
        {
            var query = _context.ReceiptDocumentTypes.Where(item => item.Id == id);
            if (!includeInactive)
                query = query.Where(item => item.IsActive);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<ReceiptDocumentType>> GetByCompanyIdAsync(Guid companyId, bool includeInactive = false)
        {
            var query = _context.ReceiptDocumentTypes.Where(item => item.CompanyId == companyId);
            if (!includeInactive)
                query = query.Where(item => item.IsActive);

            return await query.OrderBy(item => item.Name).ToListAsync();
        }

        public async Task<List<ReceiptDocumentType>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.ReceiptDocumentTypes.AsQueryable();
            if (!includeInactive)
                query = query.Where(item => item.IsActive);

            return await query.OrderBy(item => item.Name).ToListAsync();
        }

        public async Task AddAsync(ReceiptDocumentType receiptDocumentType)
        {
            await _context.ReceiptDocumentTypes.AddAsync(receiptDocumentType);
        }
    }
}
