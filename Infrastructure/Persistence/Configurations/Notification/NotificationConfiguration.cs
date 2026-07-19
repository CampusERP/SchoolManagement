using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Notification;

public class NotificationConfiguration : IEntityTypeConfiguration<Domain.Entities.Notifications.Notification>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Notifications.Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.Property(n => n.Channel).HasConversion<string>().HasMaxLength(20);
        builder.Property(n => n.Status).HasConversion<string>().HasMaxLength(20);
        // Critical for portal: "get all my unread notifications"
        builder.HasIndex(n => new { n.RecipientUserId, n.Status });
        builder.HasIndex(n => n.RecipientUserId);
        builder.Property(n => n.RowVersion).IsRowVersion();
    }
}
