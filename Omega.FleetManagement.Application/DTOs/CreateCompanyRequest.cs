namespace Omega.FleetManagement.Application.DTOs
{
    public record CreateCompanyRequest(
        string CompanyName, 
        string Cnpj, 
        string AdminFullName, 
        string AdminEmail, 
        string AdminPassword);
}
