using Omega.FleetManagement.Application.DTOs;

namespace Omega.FleetManagement.Application.Interfaces
{
    public interface IExpenseTypeAppService
    {
        Task<List<ExpenseTypeResponseDto>> GetExpenseTypesAsync(Guid? companyId, bool includeInactive = false);
        Task<ExpenseTypeResponseDto> CreateAsync(CreateExpenseTypeRequest request);
        Task<bool> UpdateAsync(Guid id, UpdateExpenseTypeRequest request);
        Task<bool> UpdateStatusAsync(Guid id, bool isActive);
    }
}
