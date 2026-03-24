using Omega.FleetManagement.Application.DTOs;

namespace Omega.FleetManagement.Application.Interfaces
{
    public interface ITripAppService
    {
        Task OpenTripAsync(OpenTripRequest request, Guid companyId);
        Task FinishTripAsync(Guid tripId, FinishTripRequest request, Guid companyId);
        Task ReopenTripAsync(Guid tripId, Guid companyId);
        Task CancelTripAsync(Guid tripId, Guid companyId);
        Task AddExpenseAsync(Guid tripId, CreateTripExpenseRequest request, Guid companyId);
        Task UpdateExpenseAsync(Guid tripId, Guid expenseId, UpdateTripExpenseRequest request, Guid companyId);
        Task<IEnumerable<TripResponseDto>> GetTripsByCompanyIdAsync(Guid CompanyId);
        Task<TripDetailResponseDto?> GetTripByIdAsync(Guid tripId, Guid companyId);
    }
}
