using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Services;
using Omega.FleetManagement.Domain.Entities;
using Omega.FleetManagement.Domain.Interfaces;
using Omega.FleetManagement.Infrastructure.Data.Context;
using Omega.FleetManagement.Infrastructure.Data.Identity;

namespace Omega.FleetManagement.Tests.Application.Services;

public class DriverAppServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly FleetContext _context;

    public DriverAppServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<FleetContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new FleetContext(options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task CreateDriverAsync_UsesIdentityUserIdAndCompanyId()
    {
        var companyId = Guid.NewGuid();
        var capturedUser = (ApplicationUser?)null;
        var persistedDriver = new Driver(companyId, Guid.NewGuid(), "Temp", "00000000000", new[] { 1m });

        var userManager = CreateUserManager();
        userManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "123456"))
            .Callback<ApplicationUser, string>((user, _) =>
            {
                user.Id = Guid.NewGuid();
                capturedUser = user;
            })
            .ReturnsAsync(IdentityResult.Success);
        userManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Driver"))
            .ReturnsAsync(IdentityResult.Success);

        var driverService = new Mock<IDriverService>();
        driverService
            .Setup(x => x.CreateDriverAsync(companyId, "Motorista Teste", "12345678901", It.Is<IEnumerable<decimal>>(rates => rates.SequenceEqual(new[] { 12.5m, 15m })), It.IsAny<Guid>()))
            .ReturnsAsync((Guid _, string name, string cpf, IEnumerable<decimal> rates, Guid userId) =>
            {
                persistedDriver = new Driver(companyId, userId, name, cpf, rates);
                return persistedDriver;
            });

        var driverRepository = new Mock<IDriverRepository>();
        driverRepository.Setup(x => x.AddAsync(It.IsAny<Driver>())).Returns(Task.CompletedTask);

        var uow = new Mock<IUnitOfWork>();
        uow.Setup(x => x.CommitAsync()).ReturnsAsync(true);

        var sut = new DriverAppService(
            _context,
            driverService.Object,
            driverRepository.Object,
            userManager.Object,
            uow.Object);

        var result = await sut.CreateDriverAsync(
            new CreateDriverRequest("Motorista Teste", "123.456.789-01", new List<decimal> { 12.5m, 15m }, "123456"),
            companyId);

        Assert.True(result);
        Assert.NotNull(capturedUser);
        Assert.Equal(companyId, capturedUser!.CompanyId);
        Assert.Equal("12345678901", capturedUser.UserName);
        Assert.Equal("12345678901.driver@omegatransportes.com", capturedUser.Email);
        Assert.Equal(capturedUser.Id, persistedDriver.UserId);

        driverRepository.Verify(x => x.AddAsync(It.Is<Driver>(d =>
            d.UserId == capturedUser.Id &&
            d.CompanyId == companyId &&
            d.Cpf == "12345678901")), Times.Once);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
    }
}
