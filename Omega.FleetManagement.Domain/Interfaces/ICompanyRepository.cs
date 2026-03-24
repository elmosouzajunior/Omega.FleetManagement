using Omega.FleetManagement.Domain.Entities;

namespace Omega.FleetManagement.Domain.Interfaces
{
    public interface ICompanyRepository
    {
        IQueryable<Company> GetAllQueryable();
        Task<Company?> GetByIdAsync(Guid id);
        Task UpdateAsync(Company company);
    }
}
