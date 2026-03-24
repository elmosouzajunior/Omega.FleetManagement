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

            // 3. Busca o percentual de comissão do motorista para o "Snapshot"
            var driver = await _driverRepository.GetByIdAsync(driverId);
            if (driver == null) throw new Exception("Motorista não encontrado.");
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
