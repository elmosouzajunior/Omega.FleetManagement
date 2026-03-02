using Omega.FleetManagement.Domain.Entities;
using Omega.FleetManagement.Domain.Interfaces;

namespace Omega.FleetManagement.Domain.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;
        
        public VehicleService(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public async Task<Vehicle> CreateVehicleAsync(Guid companyId, string licensePlate, string manufacturer, string color)
        {
            // 1. Regra: Veiculo não pode ser cadastrado mais de uma vez (validação por placa)
            var vehicleExists = await _vehicleRepository.ExistsByLicensePlateAndCompanyIdAsync(licensePlate, companyId);
            if (vehicleExists)
            {
                throw new ArgumentException("Já existe um veículo cadastrado com esta placa nesta empresa!");
            }
                        
            // 2. Cria a instância da entidade Vehicle
            var vehicle = new Vehicle(companyId, licensePlate, manufacturer, color);
            return vehicle;
        }
    }
}
