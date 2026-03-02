using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;
using Omega.FleetManagement.Domain.Interfaces;


namespace Omega.FleetManagement.Application.Services
{
    public class TripAppService : ITripAppService
    {
        private readonly ITripService _tripDomainService;
        private readonly ITripRepository _tripRepository;
        private readonly IFileStorageService _storage;
        private readonly IUnitOfWork _uow;

        public TripAppService(ITripService tripDomainService, ITripRepository tripRepository, IFileStorageService storage, IUnitOfWork uow)
        {
            _tripDomainService = tripDomainService;
            _tripRepository = tripRepository;
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
                    currentDriverId,
                    currentVehicleId,
                    dto.LoadingLocation,
                    dto.LoadingDate.ToUniversalTime(),
                    dto.StartKm, // Ajustado para Km (C# standard)
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

        public async Task<IEnumerable<TripResponseDto>> GetTripsByCompanyIdAsync(Guid companyId)
        {
            var trips = await _tripRepository.GetByCompanyIdAsync(companyId);
            var tripDtos = trips.Select(t => new TripResponseDto
            {
                Id = t.Id,
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
                FinishKm = t.FinishKm,
                FreightValue = t.FreightValue,
                CommissionPercent = t.CommissionPercent,
                CommissionValue = t.CommissionValue,
                Status = t.Status.ToString()
            }).ToList();

            return tripDtos;
        }

        public async Task<TripDetailResponseDto> GetTripByIdAsync(Guid tripId, Guid companyId)
        {
            var trip = await _tripRepository.GetByIdAsync(tripId, companyId);
            if (trip == null) return null;
            var tripDetailDto = new TripDetailResponseDto
            {
                Id = trip.Id,
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
                FinishKm = trip.FinishKm,
                FreightValue = trip.FreightValue,
                CommissionPercent = trip.CommissionPercent,
                CommissionValue = trip.CommissionValue,
                Status = trip.Status.ToString(),
                Expenses = trip.Expenses?.Select(e => new ExpenseResponseDto
                {
                    Id = e.Id,
                    Description = e.Description,
                    Value = e.Value,
                    ExpenseDate = e.Date,
                    ExpenseTypeId = e.ExpenseTypeId,
                    ExpenseTypeName = string.Empty
                }).ToList()
            };
            return tripDetailDto;
        }
    }
}
