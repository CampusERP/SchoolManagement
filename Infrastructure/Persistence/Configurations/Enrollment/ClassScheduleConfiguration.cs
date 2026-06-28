using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Enrollment;

namespace Infrastructure.Persistence.Configurations.Enrollment;

public class ClassScheduleConfiguration : IEntityTypeConfiguration<ClassSchedule>
{
    public void Configure(EntityTypeBuilder<ClassSchedule> builder)
    {
        builder.ToTable("ClassSchedules");

        builder.Property(s => s.DayOfWeek).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(s => new { s.RoomId, s.DayOfWeek, s.StartTime, s.EndTime });
        builder.HasIndex(s => s.TeachingAssignmentId);

        builder.Property(s => s.RowVersion).IsRowVersion();
    }
}
