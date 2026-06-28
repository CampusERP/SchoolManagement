using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Tenancy;

namespace Infrastructure.Persistence.Configurations.Tenancy;

public class SchoolConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {
        builder.ToTable("Schools");

        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.SubdomainCode).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Status).IsRequired().HasMaxLength(50);

        builder.HasIndex(s => s.SubdomainCode).IsUnique();

        builder.Property(s => s.RowVersion).IsRowVersion();

        builder.HasMany(s => s.Campuses)
            .WithOne()
            .HasForeignKey(c => c.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
