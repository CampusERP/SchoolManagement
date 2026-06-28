using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.People;

namespace Infrastructure.Persistence.Configurations.People;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("Teachers");

        builder.Property(t => t.EmployeeCode).IsRequired().HasMaxLength(50);
        builder.Property(t => t.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(t => t.LastName).IsRequired().HasMaxLength(100);
        builder.Property(t => t.EmploymentStatus).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(t => new { t.SchoolId, t.EmployeeCode }).IsUnique();
        builder.HasIndex(t => t.SchoolId);

        builder.Property(t => t.RowVersion).IsRowVersion();
    }
}
