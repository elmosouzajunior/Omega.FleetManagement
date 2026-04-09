using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Omega.FleetManagement.Domain.Entities;
using Omega.FleetManagement.Domain.Enums;

public class ExpenseTypeMapping : IEntityTypeConfiguration<ExpenseType>
{
    public void Configure(EntityTypeBuilder<ExpenseType> builder)
    {
        builder.ToTable("expense_types");

        builder.HasKey(et => et.Id);

        builder.Property(et => et.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(et => et.CostCategory)
               .HasConversion<int>()
               .HasColumnType("int")
               .IsRequired();
    }
}
