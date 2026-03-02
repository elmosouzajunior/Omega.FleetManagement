namespace Omega.FleetManagement.Application.DTOs
{
    public record CreateCompanyAdminRequest(
        Guid CompanyId,
        string AdminFullName, 
        string AdminCpf,
        string AdminEmail,
        string AdminPassword);
}
