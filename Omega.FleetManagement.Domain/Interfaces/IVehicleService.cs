using Omega.FleetManagement.Domain.Entities;

namespace Omega.FleetManagement.Domain.Interfaces
{
    public interface IVehicleService
    {
        Task<Vehicle> CreateVehicleAsync(Guid companyId, string licensePlate, string manufacturer, string color, decimal? loadCapacityTons);
    }
}
