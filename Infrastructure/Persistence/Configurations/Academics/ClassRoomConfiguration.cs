using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Academics;

namespace Infrastructure.Persistence.Configurations.Academics;

public class ClassRoomConfiguration : IEntityTypeConfiguration<ClassRoom>
{
    public void Configure(EntityTypeBuilder<ClassRoom> builder)
    {
        builder.ToTable("ClassRooms");

        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);

        // ClassRoom identity includes AcademicYearId — rolling
        // into a new year creates a new row instead of mutating history.
        builder.HasIndex(c => new { c.SchoolId, c.GradeLevelId, c.AcademicYearId, c.Name }).IsUnique();
        builder.HasIndex(c => c.SchoolId);
        builder.HasIndex(c => c.AcademicYearId);

        builder.Property(c => c.RowVersion).IsRowVersion();
    }
}
