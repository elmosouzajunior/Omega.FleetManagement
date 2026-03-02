namespace Omega.FleetManagement.Application.DTOs
{
    public record CompanyAdminResponse (
        string Name,
        string Cpf,
        bool IsActive,
        string CompanyName);    
}
