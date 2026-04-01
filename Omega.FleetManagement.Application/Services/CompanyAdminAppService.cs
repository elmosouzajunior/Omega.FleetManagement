using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;
using Omega.FleetManagement.Domain.Entities;
using Omega.FleetManagement.Domain.Interfaces;
using Omega.FleetManagement.Infrastructure.Data.Context;
using Omega.FleetManagement.Infrastructure.Data.Identity;
using System.ComponentModel.DataAnnotations;

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
            var admins = await _context.CompanyAdmins
                .Include(a => a.Company)
                .OrderBy(u => u.Name)
                .ToListAsync();

            var usersLookup = await _userManager.Users
                .Select(u => new { Id = u.Id.ToString(), u.Email })
                .ToDictionaryAsync(u => u.Id, u => u.Email ?? string.Empty);

            return admins
                .Select(a => new CompanyAdminResponse(
                    a.Id,
                    a.Name,
                    !string.IsNullOrWhiteSpace(a.IdentityUserId) && usersLookup.TryGetValue(a.IdentityUserId, out var email)
                        ? email
                        : a.Email,
                    a.IsActive,
                    a.Company.Name))
                .ToList();
        }

        public async Task<bool> CreateCompanyAdmin(CreateCompanyAdminRequest request)
        {
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var fullName = (request.AdminFullName ?? string.Empty).Trim();
                    var email = (request.AdminEmail ?? string.Empty).Trim().ToLowerInvariant();

                    if (string.IsNullOrWhiteSpace(fullName))
                        throw new ArgumentException("Nome é obrigatório.");

                    if (string.IsNullOrWhiteSpace(email))
                        throw new ArgumentException("E-mail é obrigatório.");

                    if (!new EmailAddressAttribute().IsValid(email))
                        throw new ArgumentException("E-mail inválido.");

                    var user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        Name = fullName,
                        CompanyId = request.CompanyId
                    };

                    var result = await _userManager.CreateAsync(user, request.AdminPassword);

                    if (!result.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }

                    await _userManager.AddToRoleAsync(user, "CompanyAdmin");

                    var companyAdmin = new CompanyAdmin
                    {
                        Id = Guid.NewGuid(),
                        Name = fullName,
                        Email = email,
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
            });
        }

        public async Task<bool> UpdateCompanyAdminAsync(Guid id, UpdateCompanyAdminRequest request)
        {
            var admin = await _context.CompanyAdmins.FirstOrDefaultAsync(a => a.Id == id);
            if (admin == null)
                return false;

            var fullName = (request.AdminFullName ?? string.Empty).Trim();
            var email = (request.AdminEmail ?? string.Empty).Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Nome é obrigatório.");

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("E-mail é obrigatório.");

            if (!new EmailAddressAttribute().IsValid(email))
                throw new ArgumentException("E-mail inválido.");

            admin.Name = fullName;
            admin.Email = email;

            if (!string.IsNullOrWhiteSpace(admin.IdentityUserId))
            {
                var user = await _userManager.FindByIdAsync(admin.IdentityUserId);
                if (user != null)
                {
                    var existingUserWithEmail = await _userManager.FindByEmailAsync(email);
                    if (existingUserWithEmail != null && existingUserWithEmail.Id != user.Id)
                        throw new ArgumentException("Já existe um administrador com este e-mail.");

                    user.Name = fullName;
                    user.UserName = email;
                    user.Email = email;
                    var identityResult = await _userManager.UpdateAsync(user);
                    if (!identityResult.Succeeded)
                    {
                        var message = identityResult.Errors.FirstOrDefault()?.Description
                            ?? "Não foi possível atualizar o administrador.";
                        throw new ArgumentException(message);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateCompanyAdminAsync(Guid id)
        {
            var admin = await _context.CompanyAdmins.FirstOrDefaultAsync(a => a.Id == id);
            if (admin == null)
                return false;

            admin.Deactivate();

            if (!string.IsNullOrWhiteSpace(admin.IdentityUserId))
            {
                var user = await _userManager.FindByIdAsync(admin.IdentityUserId);
                if (user != null)
                {
                    user.IsActive = false;
                    var identityResult = await _userManager.UpdateAsync(user);
                    if (!identityResult.Succeeded)
                    {
                        var message = identityResult.Errors.FirstOrDefault()?.Description
                            ?? "Não foi possível desativar o administrador.";
                        throw new ArgumentException(message);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReactivateCompanyAdminAsync(Guid id)
        {
            var admin = await _context.CompanyAdmins.FirstOrDefaultAsync(a => a.Id == id);
            if (admin == null)
                return false;

            admin.Reactivate();

            if (!string.IsNullOrWhiteSpace(admin.IdentityUserId))
            {
                var user = await _userManager.FindByIdAsync(admin.IdentityUserId);
                if (user != null)
                {
                    user.IsActive = true;
                    var identityResult = await _userManager.UpdateAsync(user);
                    if (!identityResult.Succeeded)
                    {
                        var message = identityResult.Errors.FirstOrDefault()?.Description
                            ?? "Não foi possível reativar o administrador.";
                        throw new ArgumentException(message);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
