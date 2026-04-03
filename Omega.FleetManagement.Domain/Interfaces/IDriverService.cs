using Omega.FleetManagement.Domain.Entities;

namespace Omega.FleetManagement.Domain.Interfaces
{
    public interface IDriverService
    {
        Task<Driver> CreateDriverAsync(Guid companyId, string name, string cpf, IEnumerable<decimal> commissionRates, Guid userId);
    }
}
