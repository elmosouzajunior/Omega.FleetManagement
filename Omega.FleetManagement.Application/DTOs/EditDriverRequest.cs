namespace Omega.FleetManagement.Application.DTOs
{
    public record EditDriverRequest(
        Guid Id,
        string Name,
        string Cpf,
        List<decimal> CommissionRates,
        bool IsActive);
}
