using Moq;
using Omega.FleetManagement.Domain.Entities;
using Omega.FleetManagement.Domain.Interfaces;
using Omega.FleetManagement.Domain.Services;

namespace Omega.FleetManagement.Tests.Domain.Services;

public class TripServiceTests
{
    private readonly Mock<ITripRepository> _tripRepository = new();
    private readonly Mock<IVehicleRepository> _vehicleRepository = new();
    private readonly Mock<IDriverRepository> _driverRepository = new();
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly Mock<IReceiptDocumentTypeRepository> _receiptDocumentTypeRepository = new();

    private TripService CreateService() =>
        new(_tripRepository.Object, _vehicleRepository.Object, _driverRepository.Object, _productRepository.Object, _receiptDocumentTypeRepository.Object);

    [Fact]
    public async Task OpenTripAsync_Throws_WhenVehicleAlreadyHasOpenTrip()
    {
        var service = CreateService();
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _tripRepository.Setup(x => x.HasOpenTripAsync(It.IsAny<Guid>(), null)).ReturnsAsync(false);
        _tripRepository.Setup(x => x.HasOpenTripByVehicleAsync(It.IsAny<Guid>(), null)).ReturnsAsync(true);
        _productRepository.Setup(x => x.GetByIdAsync(productId, false))
            .ReturnsAsync(new Product(companyId, "Soja"));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.OpenTripAsync(companyId, productId, "Cliente A", Guid.NewGuid(), Guid.NewGuid(), 10, "Origem", "Destino", DateTime.UtcNow, 100, 10, 50, 500, null));

        Assert.Equal("O veículo já possui uma viagem em andamento e não pode abrir outra.", ex.Message);
    }

    [Fact]
    public async Task OpenTripAsync_Throws_WhenDriverIsInactive()
    {
        var service = CreateService();
        var companyId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var driver = new Driver(companyId, Guid.NewGuid(), "Motorista", "12345678901", 10m);
        driver.Deactivate();

        _tripRepository.Setup(x => x.HasOpenTripAsync(driverId, null)).ReturnsAsync(false);
        _tripRepository.Setup(x => x.HasOpenTripByVehicleAsync(vehicleId, null)).ReturnsAsync(false);
        _driverRepository.Setup(x => x.GetByIdAsync(driverId, companyId)).ReturnsAsync(driver);
        _vehicleRepository.Setup(x => x.GetByIdAsync(vehicleId, companyId))
            .ReturnsAsync(new Vehicle(companyId, "ABC1234", "Volvo", "Branco", 20));
        _productRepository.Setup(x => x.GetByIdAsync(productId, false))
            .ReturnsAsync(new Product(companyId, "Soja"));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.OpenTripAsync(companyId, productId, "Cliente A", driverId, vehicleId, 10, "Origem", "Destino", DateTime.UtcNow, 100, 10, 50, 500, null));

        Assert.Equal("O motorista informado está inativo.", ex.Message);
    }

    [Fact]
    public async Task OpenTripAsync_Throws_WhenVehicleIsAssignedToAnotherDriver()
    {
        var service = CreateService();
        var companyId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var otherDriverId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var vehicle = new Vehicle(companyId, "ABC1234", "Volvo", "Branco", 20);
        vehicle.AssignDriver(otherDriverId);

        _tripRepository.Setup(x => x.HasOpenTripAsync(driverId, null)).ReturnsAsync(false);
        _tripRepository.Setup(x => x.HasOpenTripByVehicleAsync(vehicleId, null)).ReturnsAsync(false);
        _driverRepository.Setup(x => x.GetByIdAsync(driverId, companyId))
            .ReturnsAsync(new Driver(companyId, Guid.NewGuid(), "Motorista", "12345678901", 10m));
        _vehicleRepository.Setup(x => x.GetByIdAsync(vehicleId, companyId)).ReturnsAsync(vehicle);
        _productRepository.Setup(x => x.GetByIdAsync(productId, false))
            .ReturnsAsync(new Product(companyId, "Soja"));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.OpenTripAsync(companyId, productId, "Cliente A", driverId, vehicleId, 10, "Origem", "Destino", DateTime.UtcNow, 100, 10, 50, 500, null));

        Assert.Equal("O veículo está vinculado a outro motorista.", ex.Message);
    }

    [Fact]
    public async Task OpenTripAsync_Allows_OpeningWithoutLoadedWeight()
    {
        var service = CreateService();
        var companyId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var driver = new Driver(companyId, Guid.NewGuid(), "Motorista", "12345678901", 10m);
        var vehicle = new Vehicle(companyId, "ABC1234", "Volvo", "Branco", 20);
        vehicle.AssignDriver(driverId);

        _tripRepository.Setup(x => x.HasOpenTripAsync(driverId, null)).ReturnsAsync(false);
        _tripRepository.Setup(x => x.HasOpenTripByVehicleAsync(vehicleId, null)).ReturnsAsync(false);
        _driverRepository.Setup(x => x.GetByIdAsync(driverId, companyId)).ReturnsAsync(driver);
        _vehicleRepository.Setup(x => x.GetByIdAsync(vehicleId, companyId)).ReturnsAsync(vehicle);
        _productRepository.Setup(x => x.GetByIdAsync(productId, false))
            .ReturnsAsync(new Product(companyId, "Soja"));

        var trip = await service.OpenTripAsync(
            companyId, productId, "Cliente A", driverId, vehicleId, 10, "Origem", "Destino",
            DateTime.UtcNow, 100, 10, 0, 0, null);

        Assert.Equal(0, trip.LoadedWeightTons);
        Assert.Equal(0, trip.FreightValue);
    }

    [Fact]
    public async Task FinishTripAsync_Throws_WhenUnloadingDateIsBeforeLoadingDate()
    {
        var service = CreateService();
        var companyId = Guid.NewGuid();
        var loadingDate = new DateTime(2026, 3, 20, 12, 0, 0, DateTimeKind.Utc);
        var trip = new Trip(companyId, Guid.NewGuid(), "Soja", "Cliente A", Guid.NewGuid(), Guid.NewGuid(), "Origem", "Destino", loadingDate, 100, 10, 50, 500, 10m, null);

        _tripRepository.Setup(x => x.GetByIdAsync(trip.Id, companyId)).ReturnsAsync(trip);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.FinishTripAsync(trip.Id, companyId, loadingDate.AddDays(-1), "Destino", 150, 50, 500, null, Guid.NewGuid(), null, null));

        Assert.Equal("Data de encerramento não pode ser anterior à abertura da viagem.", ex.Message);
    }

    [Fact]
    public async Task FinishTripAsync_Throws_WhenTripHasNoLoadedWeight()
    {
        var service = CreateService();
        var companyId = Guid.NewGuid();
        var loadingDate = new DateTime(2026, 3, 20, 12, 0, 0, DateTimeKind.Utc);
        var receiptDocumentTypeId = Guid.NewGuid();
        var trip = new Trip(companyId, Guid.NewGuid(), "Soja", "Cliente A", Guid.NewGuid(), Guid.NewGuid(), "Origem", "Destino", loadingDate, 100, 10, 0, 0, 10m, null);

        _tripRepository.Setup(x => x.GetByIdAsync(trip.Id, companyId)).ReturnsAsync(trip);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.FinishTripAsync(trip.Id, companyId, loadingDate.AddDays(1), "Destino", 150, 50, 500, null, receiptDocumentTypeId, null, null));

        Assert.Equal("Informe o peso carregado na abertura da viagem antes de finalizar.", ex.Message);
    }
}
