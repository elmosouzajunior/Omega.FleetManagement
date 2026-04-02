namespace Omega.FleetManagement.Application.DTOs
{
    public class UpdateTripOpeningRequest
    {
        public Guid DriverId { get; set; }
        public Guid VehicleId { get; set; }
        public string LoadingLocation { get; set; } = string.Empty;
        public string UnloadingLocation { get; set; } = string.Empty;
        public DateTime LoadingDate { get; set; }
        public decimal StartKm { get; set; }
        public decimal TonValue { get; set; }
        public decimal LoadedWeightTons { get; set; }
        public decimal FreightValue { get; set; }
    }
}
