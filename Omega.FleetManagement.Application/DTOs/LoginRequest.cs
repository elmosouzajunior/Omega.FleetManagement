namespace Omega.FleetManagement.Application.DTOs
{
    public record LoginRequest(string Username, string Password);
    public record LoginResponse(string Token, string FullName, string Role, string? CompanyName);
}
