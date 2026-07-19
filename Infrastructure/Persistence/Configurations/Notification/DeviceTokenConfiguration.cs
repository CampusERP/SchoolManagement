using Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    public void Configure(EntityTypeBuilder<DeviceToken> builder)
    {
        builder.ToTable("DeviceTokens");
        builder.Property(d => d.Platform).IsRequired().HasMaxLength(20);
        builder.Property(d => d.Token).IsRequired().HasMaxLength(512);
        builder.HasIndex(d => d.Token).IsUnique();
        builder.HasIndex(d => d.ApplicationUserId);
    }
}