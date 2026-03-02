using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Omega.FleetManagement.Domain.Entities;

public class ExpenseMapping : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("expenses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description).IsRequired().HasMaxLength(250);

        builder.Property(e => e.Value).HasPrecision(18, 2);

        builder.Property(d => d.IsApproved).IsRequired().HasDefaultValue(false);

        // Shadow property para a FK da Trip (se não estiver na Entity)
        builder.Property<Guid>("TripId").IsRequired();
    }
}