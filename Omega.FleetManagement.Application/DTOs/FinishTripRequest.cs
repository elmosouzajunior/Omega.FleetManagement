namespace Omega.FleetManagement.Application.DTOs
{
    public class FinishTripRequest
    {
        public DateTime UnloadingDate { get; set; }
        public string? UnloadingLocation { get; set; }
        public decimal FinishKm { get; set; }
    }
}
