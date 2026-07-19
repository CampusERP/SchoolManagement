using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Documents;

namespace Infrastructure.Persistence.Configurations.Documents;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.Property(d => d.FileName).IsRequired().HasMaxLength(255);
        builder.Property(d => d.BlobUrl).IsRequired().HasMaxLength(1000);
        builder.Property(d => d.ContentType).IsRequired().HasMaxLength(100);

        builder.HasIndex(d => d.SchoolId);
        builder.HasIndex(d => d.UploadedByUserId);
        // BlobUrl uniqueness not enforced at DB level because the same file
        // can theoretically be uploaded twice by two different people.

        builder.Property(d => d.RowVersion).IsRowVersion();
    }
}
