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
            DateTime loadingDate,
            decimal startKm,
            decimal freightValue,
            string attachmentPath)
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

            // 4. Cria a instância da entidade Trip
            var trip = new Trip(companyId,  driverId, vehicleId, loadingLocation, loadingDate, startKm, freightValue, driver.CommissionRate, attachmentPath);

            return trip;
        }
    }
}
