namespace Omega.FleetManagement.Application.DTOs
{
    public class UpdateTripExpenseRequest
    {
        public Guid ExpenseTypeId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal? Liters { get; set; }
        public DateTime? ExpenseDate { get; set; }
    }
}
