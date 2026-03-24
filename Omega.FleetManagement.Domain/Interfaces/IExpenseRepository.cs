using Omega.FleetManagement.Domain.Entities;

namespace Omega.FleetManagement.Domain.Interfaces
{
    public interface IExpenseRepository
    {
        Task AddAsync(Expense expense);
        Task<Expense?> GetByIdAsync(Guid expenseId);
        Task UpdateAsync(Expense expense);
    }
}
