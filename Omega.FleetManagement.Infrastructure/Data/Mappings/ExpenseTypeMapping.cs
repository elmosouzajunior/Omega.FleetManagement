using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Omega.FleetManagement.Domain.Entities;

public class ExpenseTypeMapping : IEntityTypeConfiguration<ExpenseType>
{
    public void Configure(EntityTypeBuilder<ExpenseType> builder)
    {
        builder.ToTable("expense_types");

        builder.HasKey(et => et.Id);

        builder.Property(et => et.Name)
               .IsRequired()
               .HasMaxLength(100);
    }
}