using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Academics;

namespace Infrastructure.Persistence.Configurations.Academics;

public class GradeLevelConfiguration : IEntityTypeConfiguration<GradeLevel>
{
    public void Configure(EntityTypeBuilder<GradeLevel> builder)
    {
        builder.ToTable("GradeLevels");

        builder.Property(g => g.Name).IsRequired().HasMaxLength(100);

        builder.HasIndex(g => new { g.SchoolId, g.EducationStageId, g.Sequence }).IsUnique();
        builder.HasIndex(g => g.SchoolId);

        builder.Property(g => g.RowVersion).IsRowVersion();
    }
}
