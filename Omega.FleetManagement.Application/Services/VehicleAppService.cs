using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;
using Omega.FleetManagement.Domain.Interfaces;

namespace Omega.FleetManagement.Application.Services
{
    public class VehicleAppService : IVehicleAppService
    {
        private readonly IVehicleService _vehicleDomainService;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IUnitOfWork _uow;

        public VehicleAppService(IVehicleService vehicleDomainService, IVehicleRepository vehicleRepository, IUnitOfWork uow)
        {
            _vehicleDomainService = vehicleDomainService;
            _vehicleRepository = vehicleRepository;
            _uow = uow;
        }

        public async Task CreateVehicleAsync(CreateVehicleRequest dto, Guid companyId)
        {
            try
            {
                var vehicle = await _vehicleDomainService.CreateVehicleAsync(
                    companyId,
                    dto.LicensePlate,
                    dto.Manufacturer,
                    dto.Color
                );

                await _vehicleRepository.AddAsync(vehicle);
                await _uow.CommitAsync();
            }
            catch (ArgumentException ex) 
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Ocorreu um erro interno ao processar o cadastro.");
            }
        }

        public async Task<List<VehicleResponse>> GetVehiclesByCompanyIdAsync(Guid companyId)
        {
            var vehicles = await _vehicleRepository.GetByCompanyIdAsync(companyId);
            return vehicles.Select(v => new VehicleResponse
            (
                v.Id,
                v.LicensePlate,
                v.Manufacturer,
                v.Color,
                v.Driver?.Name,
                v.Driver?.Id,
                v.IsActive
            )).ToList();
        }

        public async Task<bool> AssignDriverAsync(Guid vehicleId, Guid driverId)
        {
            var isAlreadyAllocated = await _vehicleRepository.IsDriverAllocatedAsync(driverId, vehicleId);

            if (isAlreadyAllocated)
                throw new Exception("Este motorista já possui um veículo vinculado.");

            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            if (vehicle == null) return false;

            vehicle.AssignDriver(driverId);

            return await _uow.CommitAsync();
        }

        public async Task<bool> UpdateVehicleAsync(Guid id, EditVehicleRequest request)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null) return false;

            vehicle.UpdateInfo(request.LicensePlate, request.Manufacturer, request.Color, request.IsActive);
            await _vehicleRepository.UpdateAsync(vehicle);
            await _uow.CommitAsync();

            return true;
        }
    }
}
