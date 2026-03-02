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

        public async Task<bool> CreateDriverAsync(CreateDriverRequest request, Guid companyId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Criar usuário no Identity
                var user = new ApplicationUser
                {
                    UserName = request.Cpf,
                    Email = $"{request.Name}.driver@omegatransportes.com",
                    Name = request.Name
                };

                // 2. Salvar no Identity
                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                // 3. Adicionar a Role de Driver
                await _userManager.AddToRoleAsync(user, "Driver");

                // 4. Criar o Motorista no domínio
                var driver = await _driverDomainService.CreateDriverAsync(
                    companyId,
                    request.Name,
                    request.Cpf,
                    request.CommissionRate,
                    Guid.NewGuid()
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

        public async Task<bool> UpdateDriverAsync(Guid id, EditDriverRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Buscar o motorista no domínio
                var driver = await _driverRepository.GetByIdAsync(id);
                if (driver == null) return false;

                // 2. Buscar o usuário correspondente no Identity (pelo CPF antigo/UserName atual)
                var user = await _userManager.FindByNameAsync(driver.Cpf);
                if (user != null)
                {
                    // Atualiza as informações de login
                    user.UserName = request.Cpf; // CPF é o login
                    user.Name = request.Name;

                    var identityResult = await _userManager.UpdateAsync(user);

                    if (!identityResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }
                }

                // 3. Atualizar no banco de dados do domínio (Frota)
                driver.UpdateInfo(request.Name, request.Cpf, request.CommissionRate, request.IsActive);

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