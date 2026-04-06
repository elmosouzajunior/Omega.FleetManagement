using Omega.FleetManagement.Domain.Entities;

namespace Omega.FleetManagement.Domain.Interfaces
{
    public interface IReceiptDocumentTypeRepository
    {
        Task<ReceiptDocumentType?> GetByIdAsync(Guid id, bool includeInactive = false);
        Task<List<ReceiptDocumentType>> GetByCompanyIdAsync(Guid companyId, bool includeInactive = false);
        Task<List<ReceiptDocumentType>> GetAllAsync(bool includeInactive = false);
        Task AddAsync(ReceiptDocumentType receiptDocumentType);
    }
}
