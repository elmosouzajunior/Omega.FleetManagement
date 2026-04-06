using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Omega.FleetManagement.Domain.Entities;
using Omega.FleetManagement.Infrastructure.Data.Identity;

namespace Omega.FleetManagement.Infrastructure.Data.Context
{
    public class FleetContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public FleetContext(DbContextOptions<FleetContext> options) : base(options) { }

        // DbSets das nossas entidades principais
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<DriverCommission> DriverCommissions { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyAdmin> CompanyAdmins { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<ExpenseType> ExpenseTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FleetContext).Assembly);           
        }
    }
}
