using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Omega.FleetManagement.Domain.Entities;

namespace Omega.FleetManagement.Infrastructure.Data.Mappings
{
    public class VehicleMapping : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            builder.ToTable("vehicles");
            builder.HasKey(v => v.Id).HasName("pk_vehicles");

            builder.Property(v => v.LicensePlate)
                .HasMaxLength(7)
                .IsFixedLength()
                .IsRequired();

            builder.Property(v => v.Manufacturer)
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(v => v.Color)
                .HasMaxLength(30);

            builder.Property(v => v.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(v => v.CompanyId)
                .IsRequired();

            builder.Property(v => v.DriverId)
                .IsRequired(false);

            builder.HasOne(v => v.Driver)         
                .WithMany()                       
                .HasForeignKey(v => v.DriverId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(v => v.LicensePlate).IsUnique();
        }
    }
}
