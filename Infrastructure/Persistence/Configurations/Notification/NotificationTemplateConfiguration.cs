using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Notification;

// ── NOTIFICATIONS ─────────────────────────────────────────────────
public class NotificationTemplateConfiguration : IEntityTypeConfiguration<Domain.Entities.Notifications.NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Notifications.NotificationTemplate> builder)
    {
        builder.ToTable("NotificationTemplates");
        builder.Property(t => t.Type).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Channel).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.SubjectTemplate).IsRequired().HasMaxLength(200);
        builder.Property(t => t.BodyTemplate).IsRequired().HasMaxLength(4000);
        builder.HasIndex(t => new { t.SchoolId, t.Type });
    }
}
