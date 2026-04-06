using Omega.FleetManagement.Application.DTOs;

namespace Omega.FleetManagement.Application.Interfaces
{
    public interface IProductAppService
    {
        Task<List<ProductResponseDto>> GetProductsAsync(Guid? companyId, bool includeInactive = false);
        Task<ProductResponseDto> CreateAsync(CreateProductRequest request);
        Task<bool> UpdateAsync(Guid id, UpdateProductRequest request);
        Task<bool> UpdateStatusAsync(Guid id, bool isActive);
    }
}
