using Omega.FleetManagement.Domain.Entities;
using Omega.FleetManagement.Domain.Interfaces;

namespace Omega.FleetManagement.Domain.Services
{
    public class DriverService : IDriverService
    {
        private readonly IDriverRepository _driverRepository;
        
        public DriverService(IDriverRepository driverRepository)
        {
            _driverRepository = driverRepository;
        }

        public async Task<Driver> CreateDriverAsync(Guid companyId, string name, string cpf, IEnumerable<decimal> commissionRates, Guid userId)
        {
            // 1. Regra: Motorista não pode ser cadastrado mais de uma vez (validação por CPF)
            var driverExists = await _driverRepository.ExistsByCpfAndCompanyIdAsync(cpf, companyId);
            if (driverExists)
            {
                throw new ArgumentException("Já existe um motorista cadastrado com este CPF nesta empresa!");
            }
                        
            // 2. Cria a instância da entidade Driver
            var driver = new Driver(companyId, userId, name, cpf, commissionRates);

            return driver;
        }
    }
}
