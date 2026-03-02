using Omega.FleetManagement.Domain.Entities;

namespace Omega.FleetManagement.Domain.Interfaces
{
    public interface ICompanyRepository
    {
        IQueryable<Company> GetAllQueryable();
    }
}
