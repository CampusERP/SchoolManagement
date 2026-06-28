using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Academics;

namespace Infrastructure.Persistence.Configurations.Academics;

public class AcademicYearConfiguration : IEntityTypeConfiguration<AcademicYear>
{
    public void Configure(EntityTypeBuilder<AcademicYear> builder)
    {
        builder.ToTable("AcademicYears");

        builder.Property(y => y.Name).IsRequired().HasMaxLength(50);
        builder.Property(y => y.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(y => new { y.SchoolId, y.StartDate }).IsUnique();
        builder.HasIndex(y => y.SchoolId).HasDatabaseName("IX_AcademicYears_SchoolId");

        // Filtered unique index: at most one current year per school. This
        // is the database-level backstop for the invariant AcademicYear
        builder.HasIndex(y => y.SchoolId)
            .HasDatabaseName("IX_AcademicYears_OneCurrentPerSchool")
            .IsUnique()
            .HasFilter("[IsCurrent] = 1");

        builder.HasMany(y => y.Terms)
            .WithOne()
            .HasForeignKey(t => t.AcademicYearId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(y => y.RowVersion).IsRowVersion();
    }
}
