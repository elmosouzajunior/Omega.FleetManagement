namespace Omega.FleetManagement.Application.DTOs
{
    public record CompanyResponse (
        Guid Id,
        string Name,
        string? Cnpj,
        bool IsActive);    
}
