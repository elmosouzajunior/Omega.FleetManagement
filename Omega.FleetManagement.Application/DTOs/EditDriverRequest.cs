namespace Omega.FleetManagement.Application.DTOs
{
    public record EditDriverRequest(
        Guid Id,
        string Name,
        string Cpf,
        decimal CommissionRate,
        bool IsActive);    
}
