using Omega.FleetManagement.Application.DTOs;

namespace Omega.FleetManagement.Application.Interfaces
{
    public interface ICompanyAdminAppService
    {
        Task<bool> CreateCompanyAdmin(CreateCompanyAdminRequest dto);
        Task<List<CompanyAdminResponse>> GetAllCompanyAdmins();
    }
}
