using Omega.FleetManagement.Domain.Interfaces;
using Omega.FleetManagement.Infrastructure.Data.Context;

namespace Omega.FleetManagement.Infrastructure.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FleetContext _context;

        public UnitOfWork(FleetContext context)
        {
            _context = context;
        }

        public async Task<bool> CommitAsync()
        {
            // Retorna true se houver pelo menos uma linha afetada no banco
            return await _context.SaveChangesAsync() > 0;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
