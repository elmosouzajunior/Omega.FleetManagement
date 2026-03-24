using Omega.FleetManagement.Application.DTOs;

namespace Omega.FleetManagement.Application.Interfaces
{
    public interface ICompanyAdminAppService
    {
        Task<bool> CreateCompanyAdmin(CreateCompanyAdminRequest dto);
        Task<List<CompanyAdminResponse>> GetAllCompanyAdmins();
        Task<bool> UpdateCompanyAdminAsync(Guid id, UpdateCompanyAdminRequest dto);
        Task<bool> DeactivateCompanyAdminAsync(Guid id);
        Task<bool> ReactivateCompanyAdminAsync(Guid id);
    }
}
