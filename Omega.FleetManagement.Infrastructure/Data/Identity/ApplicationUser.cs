using Microsoft.AspNetCore.Identity;

namespace Omega.FleetManagement.Infrastructure.Data.Identity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public Guid? CompanyId { get; set; }
        public bool IsActive { get; set; } = true;
        public string Name { get; set; } = string.Empty;
    }
}
