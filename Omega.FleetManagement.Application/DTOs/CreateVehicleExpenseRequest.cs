namespace Omega.FleetManagement.Application.DTOs
{
    public class CreateVehicleExpenseRequest
    {
        public Guid ExpenseTypeId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public DateTime? ExpenseDate { get; set; }
    }
}
