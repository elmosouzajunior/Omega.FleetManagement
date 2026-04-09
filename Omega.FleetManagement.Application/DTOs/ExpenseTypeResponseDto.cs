namespace Omega.FleetManagement.Application.DTOs
{
    public class ExpenseTypeResponseDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CostCategory { get; set; }
        public string CostCategoryLabel { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
