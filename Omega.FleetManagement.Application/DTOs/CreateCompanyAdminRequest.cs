namespace Omega.FleetManagement.Application.DTOs
{
    public record CreateCompanyAdminRequest(
        Guid CompanyId,
        string AdminFullName, 
        string AdminEmail,
        string AdminPassword);
}
