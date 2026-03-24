namespace Omega.FleetManagement.Application.DTOs
{
    public record CompanyAdminResponse(
        Guid Id,
        string Name,
        string Email,
        bool IsActive,
        string CompanyName);
}
