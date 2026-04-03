namespace Omega.FleetManagement.Application.DTOs
{
    public record DriverResponse(
        Guid Id,
        string Name,
        string Cpf,
        decimal CommissionRate,
        List<decimal> CommissionRates,
        bool IsActive);
}
