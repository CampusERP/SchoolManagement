using Domain.Entities.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Billing;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.Property(p => p.Amount).HasPrecision(12, 2);
        builder.Property(p => p.Method).HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.Reference).HasMaxLength(200);
        builder.HasIndex(p => p.InvoiceId);
    }
}
