using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.People;

namespace Infrastructure.Persistence.Configurations.People;

public class SchoolAdminProfileConfiguration : IEntityTypeConfiguration<SchoolAdminProfile>
{
    public void Configure(EntityTypeBuilder<SchoolAdminProfile> builder)
    {
        builder.ToTable("SchoolAdminProfiles");

        builder.Property(a => a.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.LastName).IsRequired().HasMaxLength(100);

        builder.HasIndex(a => new { a.SchoolId, a.ApplicationUserId }).IsUnique();

        builder.Property(a => a.RowVersion).IsRowVersion();
    }
}
