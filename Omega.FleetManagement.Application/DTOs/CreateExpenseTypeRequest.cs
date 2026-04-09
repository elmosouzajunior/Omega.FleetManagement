namespace Omega.FleetManagement.Application.DTOs
{
    public class CreateExpenseTypeRequest
    {
        public Guid CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CostCategory { get; set; }
    }
}
