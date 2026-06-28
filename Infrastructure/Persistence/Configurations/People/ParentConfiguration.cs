using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.People;

namespace Infrastructure.Persistence.Configurations.People;

public class ParentConfiguration : IEntityTypeConfiguration<Parent>
{
    public void Configure(EntityTypeBuilder<Parent> builder)
    {
        builder.ToTable("Parents");

        builder.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.LastName).IsRequired().HasMaxLength(100);

        builder.HasIndex(p => new { p.SchoolId, p.ApplicationUserId }).IsUnique();
        builder.HasIndex(p => p.SchoolId);

        builder.Property(p => p.RowVersion).IsRowVersion();
    }
}
