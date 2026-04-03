namespace Omega.FleetManagement.Application.DTOs
{
    public class TripResponseDto
    {
        public Guid Id { get; set; }
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
        public decimal? DieselKmPerLiter { get; set; }
        public decimal? ArlaKmPerLiter { get; set; }
        public decimal CommissionPercent { get; set; }
        public decimal CommissionValue { get; set; }
        public string? Status { get; set; }
    }
}
