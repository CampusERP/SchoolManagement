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
        builder.Property(s => s.SchoolId);

        // Shared catalog codes are unique globally; custom codes are unique per school.
        builder.HasIndex(s => s.Code).IsUnique().HasFilter("[SchoolId] IS NULL");
        builder.HasIndex(s => new { s.SchoolId, s.Code }).IsUnique().HasFilter("[SchoolId] IS NOT NULL");
    }
}

public class CurriculumSubjectConfiguration : IEntityTypeConfiguration<CurriculumSubject>
{
    public void Configure(EntityTypeBuilder<CurriculumSubject> builder)
    {
        builder.ToTable("CurriculumSubjects");

        // A subject can only appear once per grade per school.
        builder.HasIndex(c => new { c.SchoolId, c.GradeLevelId, c.SubjectId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(c => c.SchoolId);

        builder.Property(c => c.RowVersion).IsRowVersion();
    }
}
