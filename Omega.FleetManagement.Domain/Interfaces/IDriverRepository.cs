using Omega.FleetManagement.Domain.Entities;

namespace Omega.FleetManagement.Domain.Interfaces
{
    public interface IDriverRepository
    {
        Task<Driver?> GetByIdAsync(Guid driverId);
        Task<Driver?> GetByIdAsync(Guid driverId, Guid companyId);
        Task<Driver?> GetByCpfAsync(string cpf);
        Task<bool> ExistsByCpfAndCompanyIdAsync(string cpf, Guid companyId);
        Task AddAsync(Driver driver);
        Task<List<Driver>> GetByCompanyIdAsync(Guid companyId);
        Task UpdateAsync(Driver driver);
    }
}
