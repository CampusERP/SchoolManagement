using Domain.Entities.Exams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Exams;

// ── EXAM ─────────────────────────────────────────────────────────
public class ExamConfigurationFixed : IEntityTypeConfiguration<Exam>
{
    public void Configure(EntityTypeBuilder<Exam> builder)
    {
        builder.ToTable("Exams");
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.MaxScore).HasPrecision(6, 2);
        builder.HasIndex(e => new { e.SchoolId, e.TermId, e.SubjectId });

        // Schedules child — FK is ExamId on ExamSchedule
        builder.HasMany(e => e.Schedules)
            .WithOne()
            .HasForeignKey(s => s.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        // Results child — FK is ExamId on ExamResult (not ExamScheduleId)
        // ExamScheduleId on ExamResult is a separate FK for "which sitting"
        builder.HasMany(e => e.Results)
            .WithOne()
            .HasForeignKey(r => r.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(e => e.RowVersion).IsRowVersion();
    }
}
