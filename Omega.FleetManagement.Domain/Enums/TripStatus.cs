namespace Omega.FleetManagement.Domain.Enums
{
    public enum TripStatus
    {
        Open = 1,      // Viagem em andamento
        Finished = 2,  // Viagem concluída com sucesso
        Cancelled = 3  // Viagem cancelada (ex: erro de lançamento)
    }
}
