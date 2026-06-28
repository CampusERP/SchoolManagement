using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Enrollment;

namespace Infrastructure.Persistence.Configurations.Enrollment;

public class StudentEnrollmentConfiguration : IEntityTypeConfiguration<StudentEnrollment>
{
    public void Configure(EntityTypeBuilder<StudentEnrollment> builder)
    {
        builder.ToTable("StudentEnrollments");

        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

        // A student enrolls once per academic year.
        builder.HasIndex(e => new { e.SchoolId, e.StudentId, e.AcademicYearId }).IsUnique();

        builder.HasIndex(e => e.ClassRoomId);
        builder.HasIndex(e => e.SchoolId);

        builder.Property(e => e.RowVersion).IsRowVersion();
    }
}
