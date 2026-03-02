namespace Omega.FleetManagement.Application.DTOs
{
    public record CreateDriverRequest(string Name, string Cpf, decimal CommissionRate, string Password);
}