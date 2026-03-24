using Omega.FleetManagement.Domain.Entities;
using Omega.FleetManagement.Domain.Interfaces;

namespace Omega.FleetManagement.Domain.Services
{
    public class TripService : ITripService
    {
        private readonly ITripRepository _tripRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IDriverRepository _driverRepository;

        public TripService(ITripRepository tripRepository, IVehicleRepository vehicleRepository, IDriverRepository driverRepository)
        {
            _tripRepository = tripRepository;
            _vehicleRepository = vehicleRepository;
            _driverRepository = driverRepository;
        }

        public async Task<Trip> OpenTripAsync(
            Guid companyId,
            Guid driverId,
            Guid vehicleId,
            string loadingLocation,
            string unloadingLocation,
            DateTime loadingDate,
            decimal startKm,
            decimal freightValue,
            string? attachmentPath)
        {
            // 1. Regra: Motorista não pode ter duas viagens abertas ao mesmo tempo
            var hasOpenTrip = await _tripRepository.HasOpenTripAsync(driverId);
            if (hasOpenTrip)
            {
                throw new ArgumentException("O motorista já possui uma viagem em andamento e não pode abrir outra.");
            }

            var vehicleHasOpenTrip = await _tripRepository.HasOpenTripByVehicleAsync(vehicleId);
            if (vehicleHasOpenTrip)
            {
                throw new ArgumentException("O veículo já possui uma viagem em andamento e não pode abrir outra.");
            }

            // 3. Busca o percentual de comissão do motorista para o "Snapshot"
            var driver = await _driverRepository.GetByIdAsync(driverId, companyId);
            if (driver == null) throw new Exception("Motorista não encontrado.");
            if (!driver.IsActive) throw new ArgumentException("O motorista informado está inativo.");

            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, companyId);
            if (vehicle == null) throw new Exception("Veículo não encontrado.");
            if (!vehicle.IsActive) throw new ArgumentException("O veículo informado está inativo.");
            if (vehicle.DriverId.HasValue && vehicle.DriverId.Value != driverId)
                throw new ArgumentException("O veículo está vinculado a outro motorista.");

            if (loadingDate == default) throw new ArgumentException("Data de carregamento é obrigatória.");
            if (loadingDate > DateTime.UtcNow.AddDays(1)) throw new ArgumentException("Data de carregamento não pode ser futura.");
            if (startKm < 0) throw new ArgumentException("KM inicial não pode ser negativo.");
            if (freightValue <= 0) throw new ArgumentException("Valor do frete deve ser maior que zero.");
            if (string.IsNullOrWhiteSpace(loadingLocation)) throw new ArgumentException("Local de carregamento é obrigatório.");
            if (string.IsNullOrWhiteSpace(unloadingLocation)) throw new ArgumentException("Destino é obrigatório na abertura da viagem.");

            // 4. Cria a instância da entidade Trip
            var trip = new Trip(companyId, driverId, vehicleId, loadingLocation, unloadingLocation, loadingDate, startKm, freightValue, driver.CommissionRate, attachmentPath);

            return trip;
        }

        public async Task<Trip> ReopenTripAsync(Guid tripId, Guid companyId)
        {
            var trip = await _tripRepository.GetByIdAsync(tripId, companyId);
            if (trip == null)
                throw new ArgumentException("Viagem não encontrada.");

            if (trip.CompanyId != companyId)
                throw new ArgumentException("Viagem não pertence à empresa informada.");

            var hasAnotherOpenTrip = await _tripRepository.HasOpenTripAsync(trip.DriverId, trip.Id);
            if (hasAnotherOpenTrip)
                throw new ArgumentException("O motorista já possui outra viagem em andamento.");

            trip.Reopen();
            return trip;
        }

        public async Task<Trip> CancelTripAsync(Guid tripId, Guid companyId)
        {
            var trip = await _tripRepository.GetByIdAsync(tripId, companyId);
            if (trip == null)
                throw new ArgumentException("Viagem não encontrada.");

            if (trip.CompanyId != companyId)
                throw new ArgumentException("Viagem não pertence à empresa informada.");

            trip.Cancel();
            return trip;
        }

        public async Task<Trip> FinishTripAsync(Guid tripId, Guid companyId, DateTime unloadingDate, string? unloadingLocation, decimal finishKm)
        {
            var trip = await _tripRepository.GetByIdAsync(tripId, companyId);
            if (trip == null)
                throw new ArgumentException("Viagem não encontrada.");

            if (trip.CompanyId != companyId)
                throw new ArgumentException("Viagem não pertence à empresa informada.");

            if (unloadingDate == default)
                throw new ArgumentException("Data de encerramento é obrigatória.");
            if (unloadingDate < trip.LoadingDate)
                throw new ArgumentException("Data de encerramento não pode ser anterior à abertura da viagem.");
            if (unloadingDate > DateTime.UtcNow.AddDays(1))
                throw new ArgumentException("Data de encerramento não pode ser futura.");

            var destination = string.IsNullOrWhiteSpace(unloadingLocation)
                ? (trip.UnloadingLocation ?? string.Empty).Trim()
                : unloadingLocation.Trim();

            if (string.IsNullOrWhiteSpace(destination))
                throw new ArgumentException("Destino não informado na abertura da viagem.");

            trip.Finish(destination, unloadingDate, finishKm);
            return trip;
        }
    }
}
