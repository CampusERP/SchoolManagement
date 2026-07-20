using Domain.Entities.Exams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.ReportCards;

public class ReportCardSubjectResultConfiguration : IEntityTypeConfiguration<ReportCardSubjectResult>
{
    public void Configure(EntityTypeBuilder<ReportCardSubjectResult> builder)
    {
        builder.ToTable("ReportCardSubjectResults");
        builder.Property(r => r.SubjectName).IsRequired().HasMaxLength(150);
        builder.Property(r => r.Score).HasPrecision(6, 2);
        builder.Property(r => r.MaxScore).HasPrecision(6, 2);
        builder.Property(r => r.Grade).IsRequired().HasMaxLength(5);
        builder.HasIndex(r => r.ReportCardId);
        // Configure RowVersion as a DB-generated concurrency token
        builder.Property(r => r.RowVersion).IsRowVersion();
    }
}
