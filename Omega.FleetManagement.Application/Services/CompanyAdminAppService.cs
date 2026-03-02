using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;
using Omega.FleetManagement.Domain.Entities;
using Omega.FleetManagement.Domain.Interfaces;
using Omega.FleetManagement.Infrastructure.Data.Context;
using Omega.FleetManagement.Infrastructure.Data.Identity;
namespace Omega.FleetManagement.Application.Services
{
    public class CompanyAdminAppService : ICompanyAdminAppService
    {
        private readonly FleetContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyAdminRepository _companyAdminRepository;

        public CompanyAdminAppService(FleetContext context, UserManager<ApplicationUser> userManager, ICompanyAdminRepository companyAdminRepository)
        {
            _context = context;
            _userManager = userManager;
            _companyAdminRepository = companyAdminRepository;
        }

        public async Task<List<CompanyAdminResponse>> GetAllCompanyAdmins()
        {
            return await _context.CompanyAdmins
                .OrderBy(u => u.Name)
                .Select(u => new CompanyAdminResponse
                (
                    u.Name,
                    u.Email,
                    u.IsActive,
                    u.Company.Name
                ))
                .ToListAsync();
        }

        public async Task<bool> CreateCompanyAdmin(CreateCompanyAdminRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Criar o objeto do Identity
                var user = new ApplicationUser
                {
                    UserName = request.AdminEmail,
                    Email = request.AdminEmail,
                    Name = request.AdminFullName                    
                };

                // 2. Salvar no Identity (isso já faz o Hash da senha automaticamente)
                var result = await _userManager.CreateAsync(user, request.AdminPassword);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                // 3. Adicionar à Role de Admin de Empresa
                await _userManager.AddToRoleAsync(user, "CompanyAdmin");

                // 4. Salvar na tabela específica do Domain (CompanyAdmins)
                var companyAdmin = new CompanyAdmin
                {
                    Id = Guid.NewGuid(),
                    Name = request.AdminFullName,
                    Email = request.AdminEmail,
                    CompanyId = request.CompanyId,
                    IdentityUserId = user.Id.ToString(),
                };

                _context.CompanyAdmins.Add(companyAdmin);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}