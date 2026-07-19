using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Exams;

namespace Infrastructure.Persistence.Configurations.Exams;

public class ExamScheduleConfiguration : IEntityTypeConfiguration<ExamSchedule>
{
    public void Configure(EntityTypeBuilder<ExamSchedule> builder)
    {
        builder.ToTable("ExamSchedules");
        builder.HasIndex(s => new { s.ExamId, s.ClassRoomId }).IsUnique();
    }
}
