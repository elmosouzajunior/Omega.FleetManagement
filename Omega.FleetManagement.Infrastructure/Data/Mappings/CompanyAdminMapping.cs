using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Omega.FleetManagement.Domain.Entities;

namespace Omega.FleetManagement.Infrastructure.Data.Mappings
{
    public class CompanyAdminMapping : IEntityTypeConfiguration<CompanyAdmin>
    {
        public void Configure(EntityTypeBuilder<CompanyAdmin> builder)
        {
            builder.ToTable("company_admins");
            builder.HasKey(d => d.Id).HasName("pk_company_admins");

            builder.Property(d => d.Name)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(d => d.Email)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(d => d.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.HasIndex(d => d.Email)
                .IsUnique();                
        }
    }
}