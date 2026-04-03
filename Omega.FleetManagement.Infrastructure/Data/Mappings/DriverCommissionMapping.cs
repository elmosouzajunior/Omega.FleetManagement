using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Omega.FleetManagement.Domain.Entities;

namespace Omega.FleetManagement.Infrastructure.Data.Mappings
{
    public class DriverCommissionMapping : IEntityTypeConfiguration<DriverCommission>
    {
        public void Configure(EntityTypeBuilder<DriverCommission> builder)
        {
            builder.ToTable("driver_commissions");
            builder.HasKey(c => c.Id).HasName("pk_driver_commissions");

            builder.Property(c => c.Rate)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            builder.Property(c => c.DriverId)
                .IsRequired();

            builder.HasIndex(c => new { c.DriverId, c.Rate })
                .IsUnique();

            builder.HasOne(c => c.Driver)
                .WithMany(d => d.Commissions)
                .HasForeignKey(c => c.DriverId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
