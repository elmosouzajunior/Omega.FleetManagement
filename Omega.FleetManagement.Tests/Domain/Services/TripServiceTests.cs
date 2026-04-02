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

    private TripService CreateService() =>
        new(_tripRepository.Object, _vehicleRepository.Object, _driverRepository.Object);

    [Fact]
    public async Task OpenTripAsync_Throws_WhenVehicleAlreadyHasOpenTrip()
    {
        var service = CreateService();

        _tripRepository.Setup(x => x.HasOpenTripAsync(It.IsAny<Guid>(), null)).ReturnsAsync(false);
        _tripRepository.Setup(x => x.HasOpenTripByVehicleAsync(It.IsAny<Guid>(), null)).ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.OpenTripAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Origem", "Destino", DateTime.UtcNow, 100, 10, 50, 500, null));

        Assert.Equal("O veículo já possui uma viagem em andamento e não pode abrir outra.", ex.Message);
    }

    [Fact]
    public async Task OpenTripAsync_Throws_WhenDriverIsInactive()
    {
        var service = CreateService();
        var companyId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var driver = new Driver(companyId, Guid.NewGuid(), "Motorista", "12345678901", 10m);
        driver.Deactivate();

        _tripRepository.Setup(x => x.HasOpenTripAsync(driverId, null)).ReturnsAsync(false);
        _tripRepository.Setup(x => x.HasOpenTripByVehicleAsync(vehicleId, null)).ReturnsAsync(false);
        _driverRepository.Setup(x => x.GetByIdAsync(driverId, companyId)).ReturnsAsync(driver);
        _vehicleRepository.Setup(x => x.GetByIdAsync(vehicleId, companyId))
            .ReturnsAsync(new Vehicle(companyId, "ABC1234", "Volvo", "Branco", 20));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.OpenTripAsync(companyId, driverId, vehicleId, "Origem", "Destino", DateTime.UtcNow, 100, 10, 50, 500, null));

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
        var vehicle = new Vehicle(companyId, "ABC1234", "Volvo", "Branco", 20);
        vehicle.AssignDriver(otherDriverId);

        _tripRepository.Setup(x => x.HasOpenTripAsync(driverId, null)).ReturnsAsync(false);
        _tripRepository.Setup(x => x.HasOpenTripByVehicleAsync(vehicleId, null)).ReturnsAsync(false);
        _driverRepository.Setup(x => x.GetByIdAsync(driverId, companyId))
            .ReturnsAsync(new Driver(companyId, Guid.NewGuid(), "Motorista", "12345678901", 10m));
        _vehicleRepository.Setup(x => x.GetByIdAsync(vehicleId, companyId)).ReturnsAsync(vehicle);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.OpenTripAsync(companyId, driverId, vehicleId, "Origem", "Destino", DateTime.UtcNow, 100, 10, 50, 500, null));

        Assert.Equal("O veículo está vinculado a outro motorista.", ex.Message);
    }

    [Fact]
    public async Task FinishTripAsync_Throws_WhenUnloadingDateIsBeforeLoadingDate()
    {
        var service = CreateService();
        var companyId = Guid.NewGuid();
        var loadingDate = new DateTime(2026, 3, 20, 12, 0, 0, DateTimeKind.Utc);
        var trip = new Trip(companyId, Guid.NewGuid(), Guid.NewGuid(), "Origem", "Destino", loadingDate, 100, 10, 50, 500, 10m, null);

        _tripRepository.Setup(x => x.GetByIdAsync(trip.Id, companyId)).ReturnsAsync(trip);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.FinishTripAsync(trip.Id, companyId, loadingDate.AddDays(-1), "Destino", 150, null, null));

        Assert.Equal("Data de encerramento não pode ser anterior à abertura da viagem.", ex.Message);
    }
}
