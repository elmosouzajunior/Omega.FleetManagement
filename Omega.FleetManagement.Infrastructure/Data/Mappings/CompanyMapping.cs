using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Omega.FleetManagement.Domain.Entities;

namespace Omega.FleetManagement.Infrastructure.Data.Mappings
{
    public class CompanyMapping : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("companies");
            builder.HasKey(d => d.Id).HasName("pk_companies");

            builder.Property(d => d.Name)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(d => d.Cnpj)
                .HasMaxLength(14)
                .IsFixedLength()
                .IsRequired();

            builder.Property(d => d.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
        }
    }
}
