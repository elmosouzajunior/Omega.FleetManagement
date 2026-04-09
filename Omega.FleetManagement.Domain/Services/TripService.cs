using Omega.FleetManagement.Domain.Entities;
using Omega.FleetManagement.Domain.Interfaces;

namespace Omega.FleetManagement.Domain.Services
{
    public class TripService : ITripService
    {
        private readonly ITripRepository _tripRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IProductRepository _productRepository;
        private readonly IReceiptDocumentTypeRepository _receiptDocumentTypeRepository;

        public TripService(ITripRepository tripRepository, IVehicleRepository vehicleRepository, IDriverRepository driverRepository, IProductRepository productRepository, IReceiptDocumentTypeRepository receiptDocumentTypeRepository)
        {
            _tripRepository = tripRepository;
            _vehicleRepository = vehicleRepository;
            _driverRepository = driverRepository;
            _productRepository = productRepository;
            _receiptDocumentTypeRepository = receiptDocumentTypeRepository;
        }

        public async Task<Trip> OpenTripAsync(
            Guid companyId,
            Guid productId,
            string clientName,
            Guid driverId,
            Guid vehicleId,
            decimal commissionPercent,
            string loadingLocation,
            string unloadingLocation,
            DateTime loadingDate,
            decimal startKm,
            decimal tonValue,
            decimal loadedWeightTons,
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
            if (!driver.HasCommissionRate(commissionPercent))
                throw new ArgumentException("A comissão selecionada não pertence ao motorista informado.");

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null || product.CompanyId != companyId)
                throw new ArgumentException("Produto invalido para a empresa.");
            if (!product.IsActive)
                throw new ArgumentException("O produto informado esta inativo.");

            if (loadingDate == default) throw new ArgumentException("Data de carregamento é obrigatória.");
            if (loadingDate > DateTime.UtcNow.AddDays(1)) throw new ArgumentException("Data de carregamento não pode ser futura.");
            if (startKm < 0) throw new ArgumentException("KM inicial não pode ser negativo.");
            if (tonValue <= 0) throw new ArgumentException("Valor da tonelada deve ser maior que zero.");
            if (loadedWeightTons < 0) throw new ArgumentException("Peso carregado nao pode ser negativo.");
            if (freightValue < 0) throw new ArgumentException("Valor do frete nao pode ser negativo.");
            if (productId == Guid.Empty) throw new ArgumentException("Produto e obrigatorio.");
            if (string.IsNullOrWhiteSpace(clientName)) throw new ArgumentException("Cliente e obrigatorio.");
            if (string.IsNullOrWhiteSpace(loadingLocation)) throw new ArgumentException("Local de carregamento é obrigatório.");
            if (string.IsNullOrWhiteSpace(unloadingLocation)) throw new ArgumentException("Destino é obrigatório na abertura da viagem.");

            // 4. Cria a instância da entidade Trip
            var trip = new Trip(
                companyId,
                product.Id,
                product.Name,
                clientName.Trim(),
                driverId,
                vehicleId,
                loadingLocation,
                unloadingLocation,
                loadingDate,
                startKm,
                tonValue,
                loadedWeightTons,
                freightValue,
                commissionPercent,
                attachmentPath);

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

        public async Task<Trip> UpdateTripOpeningAsync(
            Guid tripId,
            Guid companyId,
            Guid productId,
            string clientName,
            Guid driverId,
            Guid vehicleId,
            string loadingLocation,
            string unloadingLocation,
            DateTime loadingDate,
            decimal startKm,
            decimal tonValue,
            decimal loadedWeightTons,
            decimal freightValue)
        {
            var trip = await _tripRepository.GetByIdAsync(tripId, companyId);
            if (trip == null)
                throw new ArgumentException("Viagem não encontrada.");

            if (trip.CompanyId != companyId)
                throw new ArgumentException("Viagem não pertence à empresa informada.");

            if (trip.Status != Enums.TripStatus.Open)
                throw new ArgumentException("Somente viagens abertas podem ter a abertura editada.");

            var hasOpenTrip = await _tripRepository.HasOpenTripAsync(driverId, trip.Id);
            if (hasOpenTrip)
                throw new ArgumentException("O motorista já possui outra viagem em andamento.");

            var vehicleHasOpenTrip = await _tripRepository.HasOpenTripByVehicleAsync(vehicleId, trip.Id);
            if (vehicleHasOpenTrip)
                throw new ArgumentException("O veículo já possui outra viagem em andamento.");

            var driver = await _driverRepository.GetByIdAsync(driverId, companyId);
            if (driver == null) throw new Exception("Motorista não encontrado.");
            if (!driver.IsActive) throw new ArgumentException("O motorista informado está inativo.");

            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, companyId);
            if (vehicle == null) throw new Exception("Veículo não encontrado.");
            if (!vehicle.IsActive) throw new ArgumentException("O veículo informado está inativo.");
            if (vehicle.DriverId.HasValue && vehicle.DriverId.Value != driverId)
                throw new ArgumentException("O veículo está vinculado a outro motorista.");

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null || product.CompanyId != companyId)
                throw new ArgumentException("Produto invalido para a empresa.");
            if (!product.IsActive)
                throw new ArgumentException("O produto informado esta inativo.");

            if (loadingDate == default) throw new ArgumentException("Data de carregamento é obrigatória.");
            if (loadingDate > DateTime.UtcNow.AddDays(1)) throw new ArgumentException("Data de carregamento não pode ser futura.");
            if (startKm < 0) throw new ArgumentException("KM inicial não pode ser negativo.");
            if (tonValue <= 0) throw new ArgumentException("Valor da tonelada deve ser maior que zero.");
            if (loadedWeightTons <= 0) throw new ArgumentException("Peso carregado deve ser maior que zero.");
            if (freightValue <= 0) throw new ArgumentException("Valor do frete deve ser maior que zero.");
            if (productId == Guid.Empty) throw new ArgumentException("Produto e obrigatorio.");
            if (string.IsNullOrWhiteSpace(clientName)) throw new ArgumentException("Cliente e obrigatorio.");
            if (string.IsNullOrWhiteSpace(loadingLocation)) throw new ArgumentException("Local de carregamento é obrigatório.");
            if (string.IsNullOrWhiteSpace(unloadingLocation)) throw new ArgumentException("Destino é obrigatório na abertura da viagem.");

            trip.UpdateOpening(
                product.Id,
                product.Name,
                clientName.Trim(),
                driverId,
                vehicleId,
                loadingLocation.Trim(),
                unloadingLocation.Trim(),
                loadingDate,
                startKm,
                tonValue,
                loadedWeightTons,
                freightValue);

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

        public async Task<Trip> FinishTripAsync(Guid tripId, Guid companyId, DateTime unloadingDate, string? unloadingLocation, decimal finishKm, decimal unloadedWeightTons, decimal freightValue, decimal? cargoInsuranceValue, Guid? receiptDocumentTypeId, decimal? dieselKmPerLiter, decimal? arlaKmPerLiter)
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

            if (unloadedWeightTons <= 0)
                throw new ArgumentException("Peso descarregamento deve ser maior que zero.");

            if (freightValue <= 0)
                throw new ArgumentException("Valor do frete deve ser maior que zero.");

            if (trip.LoadedWeightTons <= 0)
                throw new ArgumentException("Informe o peso carregado na abertura da viagem antes de finalizar.");

            if (cargoInsuranceValue.HasValue && cargoInsuranceValue.Value < 0)
                throw new ArgumentException("Seguro da carga nao pode ser negativo.");

            if (!receiptDocumentTypeId.HasValue || receiptDocumentTypeId.Value == Guid.Empty)
                throw new ArgumentException("Tipo de documento do recebimento e obrigatorio.");

            var receiptDocumentType = await _receiptDocumentTypeRepository.GetByIdAsync(receiptDocumentTypeId.Value);
            if (receiptDocumentType == null || receiptDocumentType.CompanyId != companyId)
                throw new ArgumentException("Tipo de documento do recebimento invalido para a empresa.");
            if (!receiptDocumentType.IsActive)
                throw new ArgumentException("O tipo de documento do recebimento informado esta inativo.");

            if (dieselKmPerLiter.HasValue && dieselKmPerLiter.Value <= 0)
                throw new ArgumentException("Diesel - KM/L deve ser maior que zero.");

            if (arlaKmPerLiter.HasValue && arlaKmPerLiter.Value <= 0)
                throw new ArgumentException("Arla - KM/L deve ser maior que zero.");

            var minimumLossPercent = 0.26m / 100m;
            var minimumAllowedWeightBeforeRecalc = trip.LoadedWeightTons * (1 - minimumLossPercent);
            var shouldUseUnloadedWeight = unloadedWeightTons <= minimumAllowedWeightBeforeRecalc;
            var expectedFreightValue = shouldUseUnloadedWeight
                ? decimal.Round(trip.TonValue * unloadedWeightTons, 2, MidpointRounding.AwayFromZero)
                : trip.FreightValue;

            if (!shouldUseUnloadedWeight && freightValue != trip.FreightValue)
                throw new ArgumentException("Valor do frete so pode ser alterado quando houver quebra de peso igual ou superior a 0,26%.");

            if (!shouldUseUnloadedWeight && Math.Abs(freightValue - expectedFreightValue) > 0.01m)
                throw new ArgumentException("Valor do frete nao confere com o valor original da viagem.");

            trip.Finish(destination, unloadingDate, finishKm, unloadedWeightTons, freightValue, cargoInsuranceValue, receiptDocumentType.Id, receiptDocumentType.Name, dieselKmPerLiter, arlaKmPerLiter);
            return trip;
        }
    }
}
