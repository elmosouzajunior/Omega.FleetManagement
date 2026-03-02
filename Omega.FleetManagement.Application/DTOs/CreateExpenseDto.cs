namespace Omega.FleetManagement.Application.DTOs
{
    public class CreateExpenseDto
    {
        public Guid CompanyId { get; set; }
        public Guid TripId { get; set; }
        public Guid Id { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string? Description { get; set; }
        public decimal Value { get; set; }
        public Guid ExpenseTypeId { get; set; }
    }
}
