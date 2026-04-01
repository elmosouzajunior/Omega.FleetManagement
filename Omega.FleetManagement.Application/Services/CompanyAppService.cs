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
    public class CompanyAppService : ICompanyAppService
    {
        private readonly FleetContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyRepository _companyRepository;
        private readonly IUnitOfWork _uow;

        public CompanyAppService(
            FleetContext context,
            UserManager<ApplicationUser> userManager,
            ICompanyRepository companyRepository,
            IUnitOfWork uow)
        {
            _context = context;
            _userManager = userManager;
            _companyRepository = companyRepository;
            _uow = uow;
        }

        public async Task<bool> RegisterCompanyAndAdmin(CreateCompanyRequest request)
        {
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var company = new Company(request.CompanyName, request.Cnpj);

                    _context.Companies.Add(company);
                    await _context.SaveChangesAsync();

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

                    await _userManager.AddToRoleAsync(identityUser, "CompanyAdmin");

                    var domainAdmin = new CompanyAdmin
                    {
                        Name = request.AdminFullName,
                        Email = request.AdminEmail,
                        CompanyId = company.Id,
                        IdentityUserId = identityUser.Id.ToString()
                    };

                    _context.CompanyAdmins.Add(domainAdmin);
                    await _context.SaveChangesAsync();

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

        public Task<IEnumerable<CompanyResponse>> GetAllCompanies()
        {
            var companies = _companyRepository.GetAllQueryable()
                .OrderBy(c => c.Name)
                .Select(c => new CompanyResponse(c.Id, c.Name, c.Cnpj, c.IsActive))
                .ToList();

            return Task.FromResult<IEnumerable<CompanyResponse>>(companies);
        }

        public async Task<bool> UpdateCompanyAsync(Guid id, UpdateCompanyRequest request)
        {
            var company = await _companyRepository.GetByIdAsync(id);
            if (company == null)
                return false;

            company.Update(request.Name, request.Cnpj, request.IsActive);
            await _companyRepository.UpdateAsync(company);
            await _uow.CommitAsync();
            return true;
        }
    }
}
