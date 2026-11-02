using Domain.Entities.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Academics;

public sealed class SubjectBookConfiguration : IEntityTypeConfiguration<SubjectBook>
{
    public void Configure(EntityTypeBuilder<SubjectBook> builder)
    {
        builder.ToTable("SubjectBooks");
        builder.Property(book => book.Title).IsRequired().HasMaxLength(200);
        builder.Property(book => book.Author).HasMaxLength(150);
        builder.Property(book => book.Publisher).HasMaxLength(150);
        builder.Property(book => book.Isbn).HasMaxLength(30);
        builder.Property(book => book.Edition).HasMaxLength(50);
        builder.Property(book => book.Notes).HasMaxLength(1000);

        builder.HasOne<CurriculumSubject>()
            .WithMany()
            .HasForeignKey(book => book.CurriculumSubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(book => new { book.SchoolId, book.CurriculumSubjectId });
        builder.HasIndex(book => book.SchoolId);
        builder.Property(book => book.RowVersion).IsRowVersion();
    }
}
