using Omega.FleetManagement.Application.DTOs;

namespace Omega.FleetManagement.Application.Interfaces
{
    public interface ICompanyAppService
    {
        Task<bool> RegisterCompanyAndAdmin(CreateCompanyRequest request);
        Task<IEnumerable<CompanyResponse>> GetAllCompanies();
    }
}
