using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Tenancy;

namespace Infrastructure.Persistence.Configurations.Tenancy;

public class CampusConfiguration : IEntityTypeConfiguration<Campus>
{
    public void Configure(EntityTypeBuilder<Campus> builder)
    {
        builder.ToTable("Campuses");

        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Address).HasMaxLength(500);

        builder.HasIndex(c => c.SchoolId);
        builder.Property(c => c.RowVersion).IsRowVersion();
    }
}
