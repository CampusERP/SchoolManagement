using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Assignments;

namespace Infrastructure.Persistence.Configurations.Assignments;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("Assignments");

        builder.Property(a => a.Title).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Instructions).HasMaxLength(2000);
        builder.Property(a => a.MaxScore).HasPrecision(5, 2);

        // "All assignments for this teacher's class this term" — teacher portal query.
        builder.HasIndex(a => new { a.SchoolId, a.TeachingAssignmentId });
        builder.HasIndex(a => new { a.SchoolId, a.DueDate });

        builder.HasMany(a => a.Submissions)
            .WithOne()
            .HasForeignKey(s => s.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(a => a.RowVersion).IsRowVersion();
    }
}

public class AssignmentSubmissionConfiguration : IEntityTypeConfiguration<AssignmentSubmission>
{
    public void Configure(EntityTypeBuilder<AssignmentSubmission> builder)
    {
        builder.ToTable("AssignmentSubmissions");

        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Grade).HasPrecision(5, 2);
        builder.Property(s => s.TeacherFeedback).HasMaxLength(2000);

        // One submission per student per assignment — the DB-level backstop.
        builder.HasIndex(s => new { s.AssignmentId, s.StudentEnrollmentId }).IsUnique();
        builder.HasIndex(s => s.StudentEnrollmentId); // "my submissions" for student portal

        builder.HasMany(s => s.Documents)
            .WithOne()
            .HasForeignKey(d => d.AssignmentSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.RowVersion).IsRowVersion();
    }
}

public class AssignmentSubmissionDocumentConfiguration : IEntityTypeConfiguration<AssignmentSubmissionDocument>
{
    public void Configure(EntityTypeBuilder<AssignmentSubmissionDocument> builder)
    {
        builder.ToTable("AssignmentSubmissionDocuments");

        // A document can only be attached once per submission.
        builder.HasIndex(d => new { d.AssignmentSubmissionId, d.DocumentId }).IsUnique();
    }
}
