using Omega.FleetManagement.Application.DTOs;

namespace Omega.FleetManagement.Application.Interfaces
{
    public interface ITripAppService
    {
        Task OpenTripAsync(OpenTripRequest request, Guid companyId);
        Task<IEnumerable<TripResponseDto>> GetTripsByCompanyIdAsync(Guid CompanyId);
        Task<TripDetailResponseDto> GetTripByIdAsync(Guid tripId, Guid companyId);
    }
}
