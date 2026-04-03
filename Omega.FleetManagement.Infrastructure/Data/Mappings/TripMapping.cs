using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Omega.FleetManagement.Domain.Entities;

namespace Omega.FleetManagement.Infrastructure.Data.Mappings;

public class TripMapping : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.ToTable("trips");

        builder.HasKey(t => t.Id).HasName("pk_trips");

        // Mapeamento das propriedades decimais obrigatórias no SQL Server / PostgreSQL
        builder.Property(t => t.StartKm)
            .HasColumnName("StartKm")
            .HasColumnType("numeric(18,2)");

        builder.Property(t => t.TonValue)
            .HasColumnName("TonValue")
            .HasColumnType("numeric(18,2)")
            .HasDefaultValue(0);

        builder.Property(t => t.LoadedWeightTons)
            .HasColumnName("LoadedWeightTons")
            .HasColumnType("numeric(18,3)")
            .HasDefaultValue(0);

        builder.Property(t => t.UnloadedWeightTons)
            .HasColumnName("UnloadedWeightTons")
            .HasColumnType("numeric(18,3)")
            .IsRequired(false);

        builder.Property(t => t.FinishKm)
            .HasColumnName("FinishKm")
            .HasColumnType("numeric(18,2)")
            .HasDefaultValue(0);

        builder.Property(t => t.FreightValue)
            .HasColumnName("FreightValue")
            .HasColumnType("numeric(18,2)");

        builder.Property(t => t.DieselKmPerLiter)
            .HasColumnName("DieselKmPerLiter")
            .HasColumnType("numeric(18,2)")
            .IsRequired(false);

        builder.Property(t => t.ArlaKmPerLiter)
            .HasColumnName("ArlaKmPerLiter")
            .HasColumnType("numeric(18,2)")
            .IsRequired(false);

        builder.Property(t => t.CommissionPercent)
            .HasColumnName("CommissionPercent")
            .HasColumnType("decimal(5,2)");

        builder.Property(t => t.CommissionValue)
            .HasColumnName("CommissionValue")
            .HasColumnType("numeric(18,2)");


        builder.Property(t => t.LoadingLocation)
            .HasColumnName("LoadingLocation")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.UnloadingLocation)
            .HasColumnName("UnloadingLocation")
            .HasMaxLength(200);

        builder.Property(t => t.LoadingDate)
            .HasColumnName("LoadingDate")
            .IsRequired();

        builder.Property(t => t.UnloadingDate)
            .HasColumnName("UnloadingDate");

        // Status como Inteiro para performance
        builder.Property(t => t.Status)
            .HasColumnName("Status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.AttachmentPath)
            .HasColumnName("AttachmentPath")
            .HasMaxLength(500);

        // --- RELACIONAMENTOS ---

        builder.HasOne(t => t.Driver)      // Usa a propriedade de navegação física
            .WithMany()                    // Se o Driver não tiver ICollection<Trip>
            .HasForeignKey(t => t.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Vehicle)     // Adicionado mapeamento do Veículo
            .WithMany()
            .HasForeignKey(t => t.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Expenses)
            .WithOne(e => e.Trip)
            .HasForeignKey(e => e.TripId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- ÍNDICES ---
        builder.HasIndex(t => t.DriverId);
        builder.HasIndex(t => t.VehicleId);
        builder.HasIndex(t => t.Status);
    }
}
