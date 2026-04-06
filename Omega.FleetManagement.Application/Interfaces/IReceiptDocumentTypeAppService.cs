using Omega.FleetManagement.Application.DTOs;

namespace Omega.FleetManagement.Application.Interfaces
{
    public interface IReceiptDocumentTypeAppService
    {
        Task<List<ReceiptDocumentTypeResponseDto>> GetReceiptDocumentTypesAsync(Guid? companyId, bool includeInactive = false);
        Task<ReceiptDocumentTypeResponseDto> CreateAsync(CreateReceiptDocumentTypeRequest request);
        Task<bool> UpdateAsync(Guid id, UpdateReceiptDocumentTypeRequest request);
        Task<bool> UpdateStatusAsync(Guid id, bool isActive);
    }
}
