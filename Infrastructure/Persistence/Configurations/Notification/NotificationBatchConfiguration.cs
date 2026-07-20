using Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Notification;

public class NotificationBatchConfiguration : IEntityTypeConfiguration<NotificationBatch>
{
    public void Configure(EntityTypeBuilder<NotificationBatch> builder)
    {
        builder.ToTable("NotificationBatches");
        builder.Property(b => b.Subject).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Body).IsRequired().HasMaxLength(4000);
        builder.Property(b => b.Channel).HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.ScopeDescription).HasMaxLength(200);
        builder.HasIndex(b => new { b.SchoolId, b.CreatedAtUtc });
        builder.HasMany(b => b.Notifications)
            .WithOne().HasForeignKey(n => n.NotificationBatchId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(b => b.RowVersion).IsRowVersion();
    }
}
