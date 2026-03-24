using Omega.FleetManagement.Domain.Entities;
using Omega.FleetManagement.Domain.Interfaces;
using Omega.FleetManagement.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Omega.FleetManagement.Infrastructure.Data.Repositories
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly FleetContext _context;

        public ExpenseRepository(FleetContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Expense expense)
        {
            await _context.Expenses.AddAsync(expense);
        }

        public async Task<Expense?> GetByIdAsync(Guid expenseId)
        {
            return await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == expenseId);
        }

        public Task UpdateAsync(Expense expense)
        {
            _context.Expenses.Update(expense);
            return Task.CompletedTask;
        }
    }
}
