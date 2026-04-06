using Omega.FleetManagement.Domain.Entities;

namespace Omega.FleetManagement.Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid id, bool includeInactive = false);
        Task<List<Product>> GetByCompanyIdAsync(Guid companyId, bool includeInactive = false);
        Task<List<Product>> GetAllAsync(bool includeInactive = false);
        Task AddAsync(Product product);
    }
}
