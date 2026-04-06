namespace Omega.FleetManagement.Application.DTOs
{
    public class TripDetailResponseDto
    {
        public Guid Id { get; set; }
        public Guid? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ClientName { get; set; }
        public Guid DriverId { get; set; }
        public string? DriverName { get; set; }
        public Guid VehicleId { get; set; }
        public string? VehicleLicensePlate { get; set; }
        public string? VehicleManufacturer { get; set; }
        public string? VehicleCollor { get; set; }
        public string? LoadingLocation { get; set; }
        public string? UnloadingLocation { get; set; }
        public DateTime LoadingDate { get; set; }
        public DateTime UnloadingDate { get; set; }
        public decimal StartKm { get; set; }
        public decimal TonValue { get; set; }
        public decimal LoadedWeightTons { get; set; }
        public decimal? UnloadedWeightTons { get; set; }
        public decimal FinishKm { get; set; }
        public decimal FreightValue { get; set; }
        public decimal? CargoInsuranceValue { get; set; }
        public Guid? ReceiptDocumentTypeId { get; set; }
        public string? ReceiptDocumentTypeName { get; set; }
        public decimal? DieselKmPerLiter { get; set; }
        public decimal? ArlaKmPerLiter { get; set; }
        public decimal CommissionPercent { get; set; }
        public decimal CommissionValue { get; set; }
        public string? Status { get; set; }
        public List<ExpenseResponseDto>? Expenses { get; set; }
    }
}
