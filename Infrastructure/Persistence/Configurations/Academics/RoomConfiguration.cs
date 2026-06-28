using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Academics;

namespace Infrastructure.Persistence.Configurations.Academics;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");

        builder.Property(r => r.Name).IsRequired().HasMaxLength(100);

        builder.HasIndex(r => new { r.SchoolId, r.Name }).IsUnique();

        builder.Property(r => r.RowVersion).IsRowVersion();
    }
}
