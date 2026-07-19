using Domain.Entities.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Billing;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.Property(i => i.Amount).HasPrecision(12, 2);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(i => new { i.SchoolId, i.Status });
        builder.HasMany(i => i.Payments)
            .WithOne().HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(i => i.RowVersion).IsRowVersion();
    }
}
