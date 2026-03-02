using Microsoft.AspNetCore.Identity;
using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;
using Omega.FleetManagement.Domain.Entities;
using Omega.FleetManagement.Domain.Interfaces;
using Omega.FleetManagement.Infrastructure.Data.Context;
using Omega.FleetManagement.Infrastructure.Data.Identity;
using System.Data.Entity;

namespace Omega.FleetManagement.Application.Services
{
    public class CompanyAppService : ICompanyAppService
    {
        private readonly FleetContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyRepository _companyRepository;

        public CompanyAppService(FleetContext context, UserManager<ApplicationUser> userManager, ICompanyRepository companyRepository)
        {
            _context = context;
            _userManager = userManager;
            _companyRepository = companyRepository;
        }

        public async Task<bool> RegisterCompanyAndAdmin(CreateCompanyRequest request)
        {
            // Iniciamos uma transação para garantir atomicidade
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Criar a Entidade Company (Domain)
                var company = new Company(request.CompanyName, request.Cnpj);
                
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                // 2. Criar o Login no Identity (Infrastructure)
                var identityUser = new ApplicationUser
                {
                    UserName = request.AdminEmail,
                    Name = request.AdminFullName,
                    CompanyId = company.Id,
                    Email = request.AdminEmail
                };

                var identityResult = await _userManager.CreateAsync(identityUser, request.AdminPassword);

                if (!identityResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                // 3. Vincular a Role de Admin da Empresa
                await _userManager.AddToRoleAsync(identityUser, "CompanyAdmin");

                // 4. Criar a Entidade CompanyAdmin (Domain - Solução 2)
                var domainAdmin = new CompanyAdmin
                {
                    Name = request.AdminFullName,
                    Email = request.AdminEmail,
                    CompanyId = company.Id,
                    IdentityUserId = identityUser.Id.ToString() // A ponte entre os mundos
                };
                _context.CompanyAdmins.Add(domainAdmin);
                await _context.SaveChangesAsync();

                // Se tudo deu certo, confirmamos no banco
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<CompanyResponse>> GetAllCompanies()
        {
            return _companyRepository.GetAllQueryable()
                .OrderBy(c => c.Name)
                .Select(c => new CompanyResponse(c.Id, c.Name, c.Cnpj, c.IsActive))
                .ToList();
        }
    }
}