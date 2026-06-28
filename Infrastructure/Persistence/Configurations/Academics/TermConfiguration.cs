using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Academics;

namespace Infrastructure.Persistence.Configurations.Academics;

public class TermConfiguration : IEntityTypeConfiguration<Term>
{
    public void Configure(EntityTypeBuilder<Term> builder)
    {
        builder.ToTable("Terms");

        builder.Property(t => t.Name).IsRequired().HasMaxLength(50);

        builder.HasIndex(t => new { t.AcademicYearId, t.Sequence }).IsUnique();

        builder.Property(t => t.RowVersion).IsRowVersion();
    }
}
