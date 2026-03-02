using Microsoft.AspNetCore.Identity;
using Omega.FleetManagement.Infrastructure.Data.Identity;

namespace Omega.FleetManagement.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedData(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            // 1. Criar Roles
            string[] roles = { "Master", "CompanyAdmin", "Driver" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }

            // 2. Criar um usuário Master
            string masterEmail = "master@omega.com";
            if (await userManager.FindByNameAsync(masterEmail) == null)
            {
                var masterUser = new ApplicationUser
                {
                    CompanyId = null, // Master não pertence a nenhuma empresa
                    UserName = masterEmail,
                    Name = "Master User",
                    IsActive = true
                };
                await userManager.CreateAsync(masterUser, "Master@123"); // Senha forte para o Master
                await userManager.AddToRoleAsync(masterUser, "Master");
            }
        }
    }
}
