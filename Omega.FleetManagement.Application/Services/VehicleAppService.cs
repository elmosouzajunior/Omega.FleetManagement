using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;
using Omega.FleetManagement.Domain.Interfaces;

namespace Omega.FleetManagement.Application.Services
{
    public class VehicleAppService : IVehicleAppService
    {
        private readonly IVehicleService _vehicleDomainService;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IExpenseTypeRepository _expenseTypeRepository;
        private readonly IUnitOfWork _uow;

        public VehicleAppService(
            IVehicleService vehicleDomainService,
            IVehicleRepository vehicleRepository,
            IDriverRepository driverRepository,
            IExpenseRepository expenseRepository,
            IExpenseTypeRepository expenseTypeRepository,
            IUnitOfWork uow)
        {
            _vehicleDomainService = vehicleDomainService;
            _vehicleRepository = vehicleRepository;
            _driverRepository = driverRepository;
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
                dto.Color ?? string.Empty,
                dto.LoadCapacityTons
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
                v.LoadCapacityTons,
                v.Driver?.Name,
                v.Driver?.Id,
                v.IsActive
            )).ToList();
        }

        public async Task<bool> AssignDriverAsync(Guid vehicleId, Guid? driverId, Guid companyId)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, companyId);
            if (vehicle == null) return false;

            if (!driverId.HasValue)
            {
                vehicle.ReleaseVehicle();
                return await _uow.CommitAsync();
            }

            var driver = await _driverRepository.GetByIdAsync(driverId.Value, companyId);
            if (driver == null)
                throw new ArgumentException("Motorista não encontrado.");

            var isAlreadyAllocated = await _vehicleRepository.IsDriverAllocatedAsync(driverId.Value, companyId, vehicleId);
            if (isAlreadyAllocated)
                throw new Exception("Este motorista já possui um veículo vinculado.");

            vehicle.AssignDriver(driverId.Value);

            return await _uow.CommitAsync();
        }

        public async Task<bool> UpdateVehicleAsync(Guid id, EditVehicleRequest request, Guid companyId)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id, companyId);
            if (vehicle == null) return false;

            if (request.LoadCapacityTons.HasValue && request.LoadCapacityTons.Value <= 0)
                throw new ArgumentException("Capacidade de carga deve ser maior que zero.");

            vehicle.UpdateInfo(
                request.LicensePlate,
                request.Manufacturer ?? string.Empty,
                request.Color ?? string.Empty,
                request.LoadCapacityTons,
                request.IsActive);
            await _vehicleRepository.UpdateAsync(vehicle);
            await _uow.CommitAsync();

            return true;
        }

        public async Task AddExpenseAsync(Guid vehicleId, CreateVehicleExpenseRequest request, Guid companyId)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, companyId);
            if (vehicle == null || vehicle.CompanyId != companyId)
                throw new ArgumentException("Veículo não encontrado.");

            var expenseType = await _expenseTypeRepository.GetByIdAsync(request.ExpenseTypeId);
            if (expenseType == null || expenseType.CompanyId != companyId)
                throw new ArgumentException("Tipo de despesa inválido para a empresa.");

            var isFuelOrArla = IsFuelOrArlaExpense(expenseType.Name);
            var expense = new Domain.Entities.Expense(
                companyId: companyId,
                expenseTypeId: request.ExpenseTypeId,
                description: request.Description,
                value: ResolveExpenseValue(request.Value, request.Liters, request.PricePerLiter, isFuelOrArla),
                date: request.ExpenseDate?.ToUniversalTime() ?? DateTime.UtcNow,
                vehicleId: vehicleId);

            if (isFuelOrArla && (!request.Liters.HasValue || request.Liters.Value <= 0))
                throw new ArgumentException("Para Combustível ou Arla, informe os litros.");
            if (isFuelOrArla && (!request.PricePerLiter.HasValue || request.PricePerLiter.Value <= 0))
                throw new ArgumentException("Para Combustível ou Arla, informe o preco por litro.");

            expense.SetLiters(isFuelOrArla ? request.Liters : null);
            expense.SetPricePerLiter(isFuelOrArla ? request.PricePerLiter : null);

            await _expenseRepository.AddAsync(expense);
            await _uow.CommitAsync();
        }

        private static bool IsFuelOrArlaExpense(string? expenseTypeName)
        {
            var normalized = (expenseTypeName ?? string.Empty).Trim().ToLowerInvariant();
            return normalized.Contains("combust") || normalized.Contains("diesel") || normalized.Contains("arla");
        }

        private static decimal ResolveExpenseValue(decimal informedValue, decimal? liters, decimal? pricePerLiter, bool isFuelOrArla)
        {
            if (!isFuelOrArla)
                return informedValue;

            if (!liters.HasValue || liters.Value <= 0)
                throw new ArgumentException("Para Combustível ou Arla, informe os litros.");

            if (!pricePerLiter.HasValue || pricePerLiter.Value <= 0)
                throw new ArgumentException("Para Combustível ou Arla, informe o preco por litro.");

            return decimal.Round(liters.Value * pricePerLiter.Value, 2, MidpointRounding.AwayFromZero);
        }
    }
}
