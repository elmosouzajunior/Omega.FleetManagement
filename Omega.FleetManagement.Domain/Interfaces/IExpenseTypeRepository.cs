using Omega.FleetManagement.Domain.Entities;

namespace Omega.FleetManagement.Domain.Interfaces
{
    public interface IExpenseTypeRepository
    {
        Task<ExpenseType?> GetByIdAsync(Guid id, bool includeInactive = false);
        Task<List<ExpenseType>> GetByCompanyIdAsync(Guid companyId, bool includeInactive = false);
        Task<List<ExpenseType>> GetAllAsync(bool includeInactive = false);
        Task AddAsync(ExpenseType expenseType);
    }
}
