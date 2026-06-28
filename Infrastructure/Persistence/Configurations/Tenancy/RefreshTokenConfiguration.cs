using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Tenancy;

namespace Infrastructure.Persistence.Configurations.Tenancy;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.Property(r => r.Token).IsRequired().HasMaxLength(256);
        builder.HasIndex(r => r.Token).IsUnique();
        builder.HasIndex(r => r.ApplicationUserId);

        builder.Property(r => r.RowVersion).IsRowVersion();
    }
}
