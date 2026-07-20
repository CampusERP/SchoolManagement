using Domain.Entities.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.OutBoxMessage;

// ── OUTBOX  (PlatformDbContext) ───────────────────────────────────
public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.Property(m => m.Type).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Payload).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(m => m.Error).HasMaxLength(1000);
        // Processor query: unprocessed messages ordered by creation
        builder.HasIndex(m => new { m.ProcessedAtUtc, m.CreatedAtUtc });
        // Retry guard
        builder.HasIndex(m => m.RetryCount);
    }
}
