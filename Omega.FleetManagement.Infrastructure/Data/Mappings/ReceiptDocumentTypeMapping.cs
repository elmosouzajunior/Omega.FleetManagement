using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Omega.FleetManagement.Domain.Entities;

namespace Omega.FleetManagement.Infrastructure.Data.Mappings;

public class ReceiptDocumentTypeMapping : IEntityTypeConfiguration<ReceiptDocumentType>
{
    public void Configure(EntityTypeBuilder<ReceiptDocumentType> builder)
    {
        builder.ToTable("receipt_document_types");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(item => item.Description)
            .HasMaxLength(300);
    }
}
