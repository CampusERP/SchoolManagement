using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Academics;

namespace Infrastructure.Persistence.Configurations.Academics;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("Subjects");

        builder.Property(s => s.Code).IsRequired().HasMaxLength(20);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(150);
        builder.Property(s => s.Description).HasMaxLength(500);

        // Global lookup — no SchoolId, no filter.
        builder.HasIndex(s => s.Code).IsUnique();
    }
}

public class CurriculumSubjectConfiguration : IEntityTypeConfiguration<CurriculumSubject>
{
    public void Configure(EntityTypeBuilder<CurriculumSubject> builder)
    {
        builder.ToTable("CurriculumSubjects");

        // A subject can only appear once per grade per school.
        builder.HasIndex(c => new { c.SchoolId, c.GradeLevelId, c.SubjectId }).IsUnique();
        builder.HasIndex(c => c.SchoolId);

        builder.Property(c => c.RowVersion).IsRowVersion();
    }
}
