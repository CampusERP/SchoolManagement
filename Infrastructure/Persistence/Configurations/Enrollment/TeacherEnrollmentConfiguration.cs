using Domain.Entities.Enrollment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Enrollment;

public class TeacherEnrollmentConfiguration : IEntityTypeConfiguration<TeacherEnrollment>
{
    public void Configure(EntityTypeBuilder<TeacherEnrollment> builder)
    {
        builder.ToTable("TeacherEnrollments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EnrolledAtUtc)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.HasIndex(e => new { e.SchoolId, e.TeacherId, e.TermId })
            .IsUnique()
            .HasFilter(null);

        builder.HasIndex(e => e.ClassRoomId);
    }
}
