namespace Omega.FleetManagement.Application.DTOs
{
    public class UpdateExpenseTypeRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
