namespace Omega.FleetManagement.Application.DTOs
{
    public class ExpenseResponseDto
    {
        public Guid Id { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string? Description { get; set; }
        public decimal Value { get; set; }
        public Guid ExpenseTypeId { get; set; }
        public string? ExpenseTypeName { get; set; }
    }
}
