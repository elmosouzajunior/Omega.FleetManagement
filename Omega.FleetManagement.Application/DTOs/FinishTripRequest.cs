namespace Omega.FleetManagement.Application.DTOs
{
    public class FinishTripRequest
    {
        public DateTime UnloadingDate { get; set; }
        public string? UnloadingLocation { get; set; }
        public decimal FinishKm { get; set; }
        public decimal UnloadedWeightTons { get; set; }
        public decimal FreightValue { get; set; }
        public decimal? CargoInsuranceValue { get; set; }
        public Guid? ReceiptDocumentTypeId { get; set; }
        public decimal? DieselKmPerLiter { get; set; }
        public decimal? ArlaKmPerLiter { get; set; }
    }
}
