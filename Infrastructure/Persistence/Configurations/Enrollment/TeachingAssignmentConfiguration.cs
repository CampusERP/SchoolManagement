using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Enrollment;

namespace Infrastructure.Persistence.Configurations.Enrollment;

public class TeachingAssignmentConfiguration : IEntityTypeConfiguration<TeachingAssignment>
{
    public void Configure(EntityTypeBuilder<TeachingAssignment> builder)
    {
        builder.ToTable("TeachingAssignments");

        builder.HasIndex(a => new { a.SchoolId, a.TeacherId, a.ClassRoomId, a.SubjectId, a.TermId })
            .IsUnique();

        builder.HasIndex(a => new { a.TeacherId, a.TermId });
        builder.HasIndex(a => a.SchoolId);

        builder.HasMany(a => a.Schedules)
            .WithOne()
            .HasForeignKey(s => s.TeachingAssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(a => a.RowVersion).IsRowVersion();
    }
}
