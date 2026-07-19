using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.People;

namespace Infrastructure.Persistence.Configurations.People;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.Property(s => s.StudentCode).IsRequired().HasMaxLength(50);
        builder.Property(s => s.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.LastName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.NationalId).HasMaxLength(50);

        builder.HasIndex(s => new { s.SchoolId, s.StudentCode }).IsUnique();
        builder.HasIndex(s => s.SchoolId);

        builder.Property(s => s.RowVersion).IsRowVersion();
    }
}
