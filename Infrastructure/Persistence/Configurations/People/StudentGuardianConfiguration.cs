using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.People;

namespace Infrastructure.Persistence.Configurations.People;

public class StudentGuardianConfiguration : IEntityTypeConfiguration<StudentGuardian>
{
    public void Configure(EntityTypeBuilder<StudentGuardian> builder)
    {
        builder.ToTable("StudentGuardians");

        builder.Property(g => g.RelationshipType).HasConversion<string>().HasMaxLength(20);

        // A given parent should not be linked twice to the same student.
        builder.HasIndex(g => new { g.StudentId, g.ParentId }).IsUnique();
        builder.HasIndex(g => g.StudentId);
        builder.HasIndex(g => g.ParentId);

        builder.Property(g => g.RowVersion).IsRowVersion();
    }
}
