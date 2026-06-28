using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Tenancy;

namespace Infrastructure.Persistence.Configurations.Tenancy;

public class UserSchoolMembershipConfiguration : IEntityTypeConfiguration<UserSchoolMembership>
{
    public void Configure(EntityTypeBuilder<UserSchoolMembership> builder)
    {
        builder.ToTable("UserSchoolMemberships");

        builder.HasIndex(m => new { m.ApplicationUserId, m.SchoolId }).IsUnique();
        builder.HasIndex(m => m.SchoolId);

        builder.Property(m => m.RowVersion).IsRowVersion();
    }
}
