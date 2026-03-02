using Omega.FleetManagement.Domain.Entities;

namespace Omega.FleetManagement.Domain.Interfaces
{
    public interface ICompanyAdminRepository
    {
        IQueryable<CompanyAdmin> GetAllQueryable();
    }
}
