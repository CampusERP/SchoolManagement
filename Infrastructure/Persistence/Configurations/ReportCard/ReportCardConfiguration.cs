using Domain.Entities.Exams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.ReportCards;

// ── REPORT CARD ───────────────────────────────────────────────────
public class ReportCardConfiguration : IEntityTypeConfiguration<ReportCard>
{
    public void Configure(EntityTypeBuilder<ReportCard> builder)
    {
        builder.ToTable("ReportCards");
        builder.Property(r => r.OverallPercentage).HasPrecision(5, 2);
        builder.Property(r => r.OverallGrade).IsRequired().HasMaxLength(5);
        // One report card per student per term
        builder.HasIndex(r => new { r.StudentEnrollmentId, r.TermId }).IsUnique();
        builder.HasIndex(r => new { r.SchoolId, r.TermId });
        builder.HasMany(r => r.SubjectResults)
            .WithOne().HasForeignKey(s => s.ReportCardId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(r => r.RowVersion).IsRowVersion();
    }
}
