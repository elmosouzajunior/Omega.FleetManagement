using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Omega.FleetManagement.Infrastructure.Data.Identity;

namespace Omega.FleetManagement.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedData(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IConfiguration configuration)
        {
            string[] roles = { "Master", "CompanyAdmin", "Driver" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }

            var seedMasterUser = configuration.GetValue<bool>("BootstrapAdmin:Enabled");
            if (!seedMasterUser)
                return;

            var masterEmail = configuration["BootstrapAdmin:Email"]?.Trim();
            var masterPassword = configuration["BootstrapAdmin:Password"];
            var masterName = configuration["BootstrapAdmin:Name"]?.Trim();

            if (string.IsNullOrWhiteSpace(masterEmail) || string.IsNullOrWhiteSpace(masterPassword))
                throw new InvalidOperationException("BootstrapAdmin habilitado, mas Email/Password não foram configurados.");

            if (await userManager.FindByNameAsync(masterEmail) != null)
                return;

            var masterUser = new ApplicationUser
            {
                CompanyId = null,
                UserName = masterEmail,
                Email = masterEmail,
                Name = string.IsNullOrWhiteSpace(masterName) ? "Master User" : masterName,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(masterUser, masterPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Falha ao criar usuário Master bootstrap: {errors}");
            }

            var roleResult = await userManager.AddToRoleAsync(masterUser, "Master");
            if (!roleResult.Succeeded)
            {
                var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Falha ao vincular role Master no bootstrap: {errors}");
            }
        }
    }
}
