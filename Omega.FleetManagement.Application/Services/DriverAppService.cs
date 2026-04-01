using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;
using Omega.FleetManagement.Domain.Interfaces;
using Omega.FleetManagement.Infrastructure.Data.Context;
using Omega.FleetManagement.Infrastructure.Data.Identity;

namespace Omega.FleetManagement.Application.Services
{
    public class DriverAppService : IDriverAppService
    {
        private readonly FleetContext _context;
        private readonly IDriverService _driverDomainService;
        private readonly IDriverRepository _driverRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _uow;

        public DriverAppService(
            FleetContext context,
            IDriverService driverDomainService,
            IDriverRepository driverRepository,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork uow)
        {
            _context = context;
            _driverDomainService = driverDomainService;
            _driverRepository = driverRepository;
            _userManager = userManager;
            _uow = uow;
        }

        private static string NormalizeCpf(string cpf) => cpf.Replace(".", "").Replace("-", "").Trim();

        private static string BuildDriverEmail(string cpf) => $"{cpf}.driver@omegatransportes.com";

        public async Task<bool> CreateDriverAsync(CreateDriverRequest request, Guid companyId)
        {
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var normalizedCpf = NormalizeCpf(request.Cpf);

                    var user = new ApplicationUser
                    {
                        UserName = normalizedCpf,
                        Email = BuildDriverEmail(normalizedCpf),
                        Name = request.Name,
                        CompanyId = companyId,
                        IsActive = true
                    };

                    var result = await _userManager.CreateAsync(user, request.Password);

                    if (!result.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }

                    var roleResult = await _userManager.AddToRoleAsync(user, "Driver");
                    if (!roleResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }

                    var driver = await _driverDomainService.CreateDriverAsync(
                        companyId,
                        request.Name,
                        normalizedCpf,
                        request.CommissionRate,
                        user.Id
                    );

                    await _driverRepository.AddAsync(driver);
                    await _uow.CommitAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<List<DriverResponse>> GetDriversByCompanyIdAsync(Guid companyId)
        {
            var drivers = await _driverRepository.GetByCompanyIdAsync(companyId);
            return drivers.Select(d => new DriverResponse
            (
                d.Id,
                d.Name,
                d.Cpf,
                d.CommissionRate,
                d.IsActive
            )).ToList();
        }

        public async Task<bool> UpdateDriverAsync(Guid id, EditDriverRequest request, Guid companyId)
        {
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var driver = await _driverRepository.GetByIdAsync(id, companyId);
                    if (driver == null) return false;

                    var normalizedCpf = NormalizeCpf(request.Cpf);

                    var user = await _userManager.FindByIdAsync(driver.UserId.ToString());
                    if (user == null)
                    {
                        user = await _userManager.FindByNameAsync(NormalizeCpf(driver.Cpf));
                        if (user != null)
                            driver.LinkUser(user.Id);
                    }

                    if (user != null)
                    {
                        user.UserName = normalizedCpf;
                        user.Email = BuildDriverEmail(normalizedCpf);
                        user.Name = request.Name;
                        user.CompanyId = companyId;
                        user.IsActive = request.IsActive;

                        var identityResult = await _userManager.UpdateAsync(user);

                        if (!identityResult.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            return false;
                        }
                    }

                    driver.UpdateInfo(request.Name, normalizedCpf, request.CommissionRate, request.IsActive);

                    await _driverRepository.UpdateAsync(driver);
                    await _uow.CommitAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
    }
}
