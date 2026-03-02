namespace Omega.FleetManagement.Domain.Interfaces
{
    public interface IDashboardRepository
    {
        Task<int> GetActiveDriversCountAsync(Guid companyId);
        Task<int> GetActiveVehiclesCountAsync(Guid companyId);
        Task<int> GetOpenTripsCountAsync(Guid companyId);
        //Task<List<Expense>> GetPendingExpensesAsync(Guid companyId);
    }
}
