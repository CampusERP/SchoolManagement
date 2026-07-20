using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Attendance;

namespace Infrastructure.Persistence.Configurations.Attendance;

public class AttendanceSessionConfiguration : IEntityTypeConfiguration<AttendanceSession>
{
    public void Configure(EntityTypeBuilder<AttendanceSession> builder)
    {
        builder.ToTable("AttendanceSessions");

        // One session per class-day — the DB backstop for the domain invariant.
        builder.HasIndex(s => new { s.ClassScheduleId, s.Date }).IsUnique();

        // SchoolId leads every composite index — tenant queries filter here first.
        builder.HasIndex(s => new { s.SchoolId, s.Date });

        builder.HasMany(s => s.Records)
            .WithOne()
            .HasForeignKey(r => r.AttendanceSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.RowVersion).IsRowVersion();
    }
}

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("AttendanceRecords");

        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Note).HasMaxLength(500);

        // One record per student per session — prevents duplicates.
        builder.HasIndex(r => new { r.AttendanceSessionId, r.StudentEnrollmentId }).IsUnique();

        // "Get all attendance for this student" — a very common portal query.
        // This index supports student and parent portal with a single seek.
        builder.HasIndex(r => r.StudentEnrollmentId);

        // Tenant isolation index — SchoolId leads for filtered queries
        builder.HasIndex(r => r.SchoolId);

        // NOTE: When AttendanceRecords hits ~50M rows, add SQL Server table
        // partitioning by year (using a computed column from AttendanceSession.Date).
        // Design for it now; implement at the volume threshold, not after.
        builder.Property(r => r.RowVersion).IsRowVersion();
    }
}
