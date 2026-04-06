namespace Omega.FleetManagement.Application.DTOs
{
    public class CreateTripExpenseRequest
    {
        public Guid ExpenseTypeId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal? Liters { get; set; }
        public decimal? PricePerLiter { get; set; }
        public DateTime? ExpenseDate { get; set; }
    }
}
