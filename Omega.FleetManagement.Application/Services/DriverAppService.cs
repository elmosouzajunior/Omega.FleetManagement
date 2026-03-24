using Microsoft.AspNetCore.Identity;
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
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var normalizedCpf = NormalizeCpf(request.Cpf);

                // 1. Criar usuário no Identity
                var user = new ApplicationUser
                {
                    UserName = normalizedCpf,
                    Email = BuildDriverEmail(normalizedCpf),
                    Name = request.Name,
                    CompanyId = companyId,
                    IsActive = true
                };

                // 2. Salvar no Identity
                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                // 3. Adicionar a Role de Driver
                var roleResult = await _userManager.AddToRoleAsync(user, "Driver");
                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                // 4. Criar o Motorista no domínio
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
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
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
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Buscar o motorista no domínio
                var driver = await _driverRepository.GetByIdAsync(id, companyId);
                if (driver == null) return false;

                var normalizedCpf = NormalizeCpf(request.Cpf);

                // 2. Buscar o usuário correspondente no Identity pelo vínculo persistido
                var user = await _userManager.FindByIdAsync(driver.UserId.ToString());
                if (user == null)
                {
                    // Fallback para dados legados que foram salvos com UserId inconsistente
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

                // 3. Atualizar no banco de dados do domínio (Frota)
                driver.UpdateInfo(request.Name, normalizedCpf, request.CommissionRate, request.IsActive);

                await _driverRepository.UpdateAsync(driver);
                await _uow.CommitAsync();

                // 4. Confirmar tudo
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
