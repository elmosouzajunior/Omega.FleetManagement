using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;
using Omega.FleetManagement.Domain.Interfaces;


namespace Omega.FleetManagement.Application.Services
{
    public class TripAppService : ITripAppService
    {
        private readonly ITripService _tripDomainService;
        private readonly ITripRepository _tripRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IExpenseTypeRepository _expenseTypeRepository;
        private readonly IReceiptDocumentTypeRepository _receiptDocumentTypeRepository;
        private readonly IFileStorageService _storage;
        private readonly IUnitOfWork _uow;

        public TripAppService(
            ITripService tripDomainService,
            ITripRepository tripRepository,
            IExpenseRepository expenseRepository,
            IExpenseTypeRepository expenseTypeRepository,
            IReceiptDocumentTypeRepository receiptDocumentTypeRepository,
            IFileStorageService storage,
            IUnitOfWork uow)
        {
            _tripDomainService = tripDomainService;
            _tripRepository = tripRepository;
            _expenseRepository = expenseRepository;
            _expenseTypeRepository = expenseTypeRepository;
            _receiptDocumentTypeRepository = receiptDocumentTypeRepository;
            _storage = storage;
            _uow = uow;
        }

        public async Task OpenTripAsync(OpenTripRequest dto, Guid companyId)
        {
            var currentDriverId = dto.DriverId;
            var currentVehicleId = dto.VehicleId;

            string? photoPath = null;

            if (dto.FreightAttachment != null)
            {
                using var stream = dto.FreightAttachment.OpenReadStream();
                var subFolder = "trips";
                photoPath = await _storage.UploadFileAsync(stream, dto.FreightAttachment.FileName, subFolder);
            }

            try
            {
                // Chamada ao domínio passando o StartKm (verifique o casing no seu DTO)
                var trip = await _tripDomainService.OpenTripAsync(
                    companyId,
                    dto.ProductId,
                    dto.ClientName,
                    currentDriverId,
                    currentVehicleId,
                    dto.CommissionPercent,
                    dto.LoadingLocation,
                    dto.UnloadingLocation,
                    dto.LoadingDate.ToUniversalTime(),
                    dto.StartKm, // Ajustado para Km (C# standard)
                    dto.TonValue,
                    dto.LoadedWeightTons,
                    dto.FreightValue,
                    photoPath
                );

                // Persistência Atômica
                await _tripRepository.AddAsync(trip);
                await _uow.CommitAsync();
            }
            catch (Exception)
            {
                // Rollback do arquivo físico se o banco falhar
                if (!string.IsNullOrEmpty(photoPath))
                    await _storage.DeleteFileAsync(photoPath);

                throw;
            }
        }

        public async Task UpdateTripOpeningAsync(Guid tripId, UpdateTripOpeningRequest request, Guid companyId)
        {
            var trip = await _tripDomainService.UpdateTripOpeningAsync(
                tripId,
                companyId,
                request.ProductId,
                request.ClientName,
                request.DriverId,
                request.VehicleId,
                request.LoadingLocation,
                request.UnloadingLocation,
                request.LoadingDate.ToUniversalTime(),
                request.StartKm,
                request.TonValue,
                request.LoadedWeightTons,
                request.FreightValue);

            await _tripRepository.UpdateAsync(trip);
            await _uow.CommitAsync();
        }

        public async Task ReopenTripAsync(Guid tripId, Guid companyId)
        {
            var trip = await _tripDomainService.ReopenTripAsync(tripId, companyId);
            await _tripRepository.UpdateAsync(trip);
            await _uow.CommitAsync();
        }

        public async Task CancelTripAsync(Guid tripId, Guid companyId)
        {
            var trip = await _tripDomainService.CancelTripAsync(tripId, companyId);
            await _tripRepository.UpdateAsync(trip);
            await _uow.CommitAsync();
        }

        public async Task FinishTripAsync(Guid tripId, FinishTripRequest request, Guid companyId)
        {
            if (request.DieselKmPerLiter.HasValue && request.DieselKmPerLiter.Value <= 0)
                throw new ArgumentException("Diesel - KM/L deve ser maior que zero.");

            if (request.ArlaKmPerLiter.HasValue && request.ArlaKmPerLiter.Value <= 0)
                throw new ArgumentException("Arla - KM/L deve ser maior que zero.");

            if (request.CargoInsuranceValue.HasValue && request.CargoInsuranceValue.Value < 0)
                throw new ArgumentException("Seguro da carga nao pode ser negativo.");

            var trip = await _tripDomainService.FinishTripAsync(
                tripId,
                companyId,
                request.UnloadingDate.ToUniversalTime(),
                request.UnloadingLocation,
                request.FinishKm,
                request.UnloadedWeightTons,
                request.FreightValue,
                request.CargoInsuranceValue,
                request.ReceiptDocumentTypeId,
                request.DieselKmPerLiter,
                request.ArlaKmPerLiter);

            await _tripRepository.UpdateAsync(trip);
            await _uow.CommitAsync();
        }

        public async Task AddExpenseAsync(Guid tripId, CreateTripExpenseRequest request, Guid companyId)
        {
            var trip = await _tripRepository.GetByIdAsync(tripId, companyId);
            if (trip == null || trip.CompanyId != companyId)
                throw new ArgumentException("Viagem não encontrada.");

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
                tripId: tripId);

            if (isFuelOrArla && (!request.Liters.HasValue || request.Liters.Value <= 0))
                throw new ArgumentException("Para Combustível ou Arla, informe os litros.");
            if (isFuelOrArla && (!request.PricePerLiter.HasValue || request.PricePerLiter.Value <= 0))
                throw new ArgumentException("Para Combustível ou Arla, informe o preco por litro.");

            expense.SetLiters(isFuelOrArla ? request.Liters : null);
            expense.SetPricePerLiter(isFuelOrArla ? request.PricePerLiter : null);

            trip.AddExpense(expense);

            await _expenseRepository.AddAsync(expense);
            await _uow.CommitAsync();
        }

        public async Task UpdateExpenseAsync(Guid tripId, Guid expenseId, UpdateTripExpenseRequest request, Guid companyId)
        {
            var trip = await _tripRepository.GetByIdAsync(tripId, companyId);
            if (trip == null || trip.CompanyId != companyId)
                throw new ArgumentException("Viagem não encontrada.");

            var expense = await _expenseRepository.GetByIdAsync(expenseId);
            if (expense == null || expense.CompanyId != companyId || expense.TripId != tripId)
                throw new ArgumentException("Despesa não encontrada para esta viagem.");

            var expenseType = await _expenseTypeRepository.GetByIdAsync(request.ExpenseTypeId);
            if (expenseType == null || expenseType.CompanyId != companyId)
                throw new ArgumentException("Tipo de despesa inválido para a empresa.");

            expense.SetExpenseType(request.ExpenseTypeId);
            expense.SetDescription(request.Description);
            var isFuelOrArla = IsFuelOrArlaExpense(expenseType.Name);
            if (isFuelOrArla && (!request.Liters.HasValue || request.Liters.Value <= 0))
                throw new ArgumentException("Para Combustível ou Arla, informe os litros.");
            if (isFuelOrArla && (!request.PricePerLiter.HasValue || request.PricePerLiter.Value <= 0))
                throw new ArgumentException("Para Combustível ou Arla, informe o preco por litro.");

            expense.SetValue(ResolveExpenseValue(request.Value, request.Liters, request.PricePerLiter, isFuelOrArla));
            expense.SetLiters(isFuelOrArla ? request.Liters : null);
            expense.SetPricePerLiter(isFuelOrArla ? request.PricePerLiter : null);

            if (request.ExpenseDate.HasValue)
                expense.ChangeDate(request.ExpenseDate.Value.ToUniversalTime());

            await _expenseRepository.UpdateAsync(expense);
            await _uow.CommitAsync();
        }

        public async Task<IEnumerable<TripResponseDto>> GetTripsByCompanyIdAsync(Guid companyId)
        {
            var trips = await _tripRepository.GetByCompanyIdAsync(companyId);
            var tripDtos = trips.Select(t => new TripResponseDto
            {
                Id = t.Id,
                ProductId = t.ProductId,
                ProductName = t.ProductName,
                ClientName = t.ClientName,
                DriverId = t.DriverId,
                DriverName = t.Driver?.Name ?? string.Empty,
                VehicleId = t.VehicleId,
                VehicleLicensePlate = t.Vehicle?.LicensePlate ?? string.Empty,
                VehicleManufacturer = t.Vehicle?.Manufacturer ?? string.Empty,
                VehicleCollor = t.Vehicle?.Color ?? string.Empty,
                LoadingDate = t.LoadingDate,
                LoadingLocation = t.LoadingLocation,
                UnloadingDate = t.UnloadingDate ?? DateTime.MinValue,
                UnloadingLocation = t.UnloadingLocation ?? string.Empty,
                StartKm = t.StartKm,
                TonValue = t.TonValue,
                LoadedWeightTons = t.LoadedWeightTons,
                UnloadedWeightTons = t.UnloadedWeightTons,
                FinishKm = t.FinishKm,
                FreightValue = t.FreightValue,
                CargoInsuranceValue = t.CargoInsuranceValue,
                ReceiptDocumentTypeId = t.ReceiptDocumentTypeId,
                ReceiptDocumentTypeName = t.ReceiptDocumentTypeName,
                DieselKmPerLiter = t.DieselKmPerLiter,
                ArlaKmPerLiter = t.ArlaKmPerLiter,
                CommissionPercent = t.CommissionPercent,
                CommissionValue = t.CommissionValue,
                Status = t.Status.ToString()
            }).ToList();

            return tripDtos;
        }

        public async Task<TripDetailResponseDto?> GetTripByIdAsync(Guid tripId, Guid companyId)
        {
            var trip = await _tripRepository.GetByIdAsync(tripId, companyId);
            if (trip == null) return null;
            var tripDetailDto = new TripDetailResponseDto
            {
                Id = trip.Id,
                ProductId = trip.ProductId,
                ProductName = trip.ProductName,
                ClientName = trip.ClientName,
                DriverId = trip.DriverId,
                DriverName = trip.Driver?.Name ?? string.Empty,
                VehicleId = trip.VehicleId,
                VehicleLicensePlate = trip.Vehicle?.LicensePlate ?? string.Empty,
                VehicleManufacturer = trip.Vehicle?.Manufacturer ?? string.Empty,
                VehicleCollor = trip.Vehicle?.Color ?? string.Empty,
                LoadingDate = trip.LoadingDate,
                LoadingLocation = trip.LoadingLocation,
                UnloadingDate = trip.UnloadingDate ?? DateTime.MinValue,
                UnloadingLocation = trip.UnloadingLocation ?? string.Empty,
                StartKm = trip.StartKm,
                TonValue = trip.TonValue,
                LoadedWeightTons = trip.LoadedWeightTons,
                UnloadedWeightTons = trip.UnloadedWeightTons,
                FinishKm = trip.FinishKm,
                FreightValue = trip.FreightValue,
                CargoInsuranceValue = trip.CargoInsuranceValue,
                ReceiptDocumentTypeId = trip.ReceiptDocumentTypeId,
                ReceiptDocumentTypeName = trip.ReceiptDocumentTypeName,
                DieselKmPerLiter = trip.DieselKmPerLiter,
                ArlaKmPerLiter = trip.ArlaKmPerLiter,
                CommissionPercent = trip.CommissionPercent,
                CommissionValue = trip.CommissionValue,
                Status = trip.Status.ToString(),
                Expenses = trip.Expenses?.Select(e => new ExpenseResponseDto
                {
                    Id = e.Id,
                    Description = e.Description,
                    Value = e.Value,
                    Liters = e.Liters,
                    PricePerLiter = e.PricePerLiter,
                    ExpenseDate = e.Date,
                    ExpenseTypeId = e.ExpenseTypeId,
                    ExpenseTypeName = e.ExpenseType?.Name
                }).ToList()
            };
            return tripDetailDto;
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
