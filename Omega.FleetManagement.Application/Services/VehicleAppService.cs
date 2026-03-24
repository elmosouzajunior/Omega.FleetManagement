using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;
using Omega.FleetManagement.Domain.Interfaces;

namespace Omega.FleetManagement.Application.Services
{
    public class VehicleAppService : IVehicleAppService
    {
        private readonly IVehicleService _vehicleDomainService;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IExpenseTypeRepository _expenseTypeRepository;
        private readonly IUnitOfWork _uow;

        public VehicleAppService(
            IVehicleService vehicleDomainService,
            IVehicleRepository vehicleRepository,
            IExpenseRepository expenseRepository,
            IExpenseTypeRepository expenseTypeRepository,
            IUnitOfWork uow)
        {
            _vehicleDomainService = vehicleDomainService;
            _vehicleRepository = vehicleRepository;
            _expenseRepository = expenseRepository;
            _expenseTypeRepository = expenseTypeRepository;
            _uow = uow;
        }

        public async Task CreateVehicleAsync(CreateVehicleRequest dto, Guid companyId)
        {
            var vehicle = await _vehicleDomainService.CreateVehicleAsync(
                companyId,
                dto.LicensePlate,
                dto.Manufacturer ?? string.Empty,
                dto.Color ?? string.Empty
            );

            await _vehicleRepository.AddAsync(vehicle);
            await _uow.CommitAsync();
        }

        public async Task<List<VehicleResponse>> GetVehiclesByCompanyIdAsync(Guid companyId)
        {
            var vehicles = await _vehicleRepository.GetByCompanyIdAsync(companyId);
            return vehicles.Select(v => new VehicleResponse
            (
                v.Id,
                v.LicensePlate,
                v.Manufacturer ?? string.Empty,
                v.Color ?? string.Empty,
                v.Driver?.Name,
                v.Driver?.Id,
                v.IsActive
            )).ToList();
        }

        public async Task<bool> AssignDriverAsync(Guid vehicleId, Guid? driverId)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            if (vehicle == null) return false;

            if (!driverId.HasValue)
            {
                vehicle.ReleaseVehicle();
                return await _uow.CommitAsync();
            }

            var isAlreadyAllocated = await _vehicleRepository.IsDriverAllocatedAsync(driverId.Value, vehicleId);
            if (isAlreadyAllocated)
                throw new Exception("Este motorista já possui um veículo vinculado.");

            vehicle.AssignDriver(driverId.Value);

            return await _uow.CommitAsync();
        }

        public async Task<bool> UpdateVehicleAsync(Guid id, EditVehicleRequest request)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null) return false;

            vehicle.UpdateInfo(request.LicensePlate, request.Manufacturer ?? string.Empty, request.Color ?? string.Empty, request.IsActive);
            await _vehicleRepository.UpdateAsync(vehicle);
            await _uow.CommitAsync();

            return true;
        }

        public async Task AddExpenseAsync(Guid vehicleId, CreateVehicleExpenseRequest request, Guid companyId)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            if (vehicle == null || vehicle.CompanyId != companyId)
                throw new ArgumentException("Veículo não encontrado.");

            var expenseType = await _expenseTypeRepository.GetByIdAsync(request.ExpenseTypeId);
            if (expenseType == null || expenseType.CompanyId != companyId)
                throw new ArgumentException("Tipo de despesa inválido para a empresa.");

            var expense = new Domain.Entities.Expense(
                companyId: companyId,
                expenseTypeId: request.ExpenseTypeId,
                description: request.Description,
                value: request.Value,
                date: request.ExpenseDate?.ToUniversalTime() ?? DateTime.UtcNow,
                vehicleId: vehicleId);

            await _expenseRepository.AddAsync(expense);
            await _uow.CommitAsync();
        }
    }
}
