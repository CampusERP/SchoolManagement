using Domain.Entities.Exams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Exams;

public class ExamResultConfigurationFixed : IEntityTypeConfiguration<ExamResult>
{
    public void Configure(EntityTypeBuilder<ExamResult> builder)
    {
        builder.ToTable("ExamResults");
        builder.Property(r => r.Score).HasPrecision(6, 2);
        builder.Property(r => r.Remarks).HasMaxLength(500);

        // Unique: one result per student per exam schedule (sitting)
        builder.HasIndex(r => new { r.ExamScheduleId, r.StudentEnrollmentId }).IsUnique();

        // Portal query index — "all my exam results"
        builder.HasIndex(r => r.StudentEnrollmentId);

        // ExamScheduleId is a non-navigable FK — just an index, no nav property
        builder.HasIndex(r => r.ExamScheduleId);

        builder.Property(r => r.RowVersion).IsRowVersion();
    }
}
