namespace Omega.FleetManagement.Application.DTOs
{
    public record CreateDriverRequest(string Name, string Cpf, List<decimal> CommissionRates, string Password);
}
