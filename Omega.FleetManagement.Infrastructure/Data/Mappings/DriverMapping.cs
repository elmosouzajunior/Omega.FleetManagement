using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Omega.FleetManagement.Domain.Entities;

namespace Omega.FleetManagement.Infrastructure.Data.Mappings
{
    public class DriverMapping : IEntityTypeConfiguration<Driver>
    {
        public void Configure(EntityTypeBuilder<Driver> builder)
        {
            builder.ToTable("drivers");
            builder.HasKey(d => d.Id).HasName("pk_drivers");

            builder.Property(d => d.Name)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(d => d.Cpf)
                .HasMaxLength(11)
                .IsRequired();

            builder.Property(d => d.CommissionRate)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            builder.Property(d => d.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Chaves Estrangeiras (IDs)
            builder.Property(d => d.UserId)
                .IsRequired();

            builder.Property(d => d.CompanyId)
                .IsRequired();

            // Índice único Composto (CPF + Empresa)
            builder.HasIndex(d => new { d.Cpf, d.CompanyId })
                .IsUnique();
        }
    }
}
