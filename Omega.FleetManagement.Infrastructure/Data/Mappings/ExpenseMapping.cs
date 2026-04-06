using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Omega.FleetManagement.Domain.Entities;

namespace Omega.FleetManagement.Infrastructure.Data.Mappings;

public class ExpenseMapping : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("expenses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description).IsRequired().HasMaxLength(250);
        builder.Property(e => e.Value).HasPrecision(18, 2);
        builder.Property(e => e.Liters).HasPrecision(18, 2).IsRequired(false);
        builder.Property(e => e.PricePerLiter).HasPrecision(18, 4).IsRequired(false);
        builder.Property(e => e.IsApproved).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.TripId).IsRequired(false);
        builder.Property(e => e.VehicleId).IsRequired(false);

        builder.HasOne(e => e.Trip)
            .WithMany(t => t.Expenses)
            .HasForeignKey(e => e.TripId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Vehicle)
            .WithMany(v => v.Expenses)
            .HasForeignKey(e => e.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ExpenseType)
            .WithMany(t => t.Expenses)
            .HasForeignKey(e => e.ExpenseTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
