using Domain.Entities.People;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.People;

public sealed class TeacherCampusConfiguration : IEntityTypeConfiguration<TeacherCampus>
{
    public void Configure(EntityTypeBuilder<TeacherCampus> builder)
    {
        builder.ToTable("TeacherCampuses");
        builder.HasIndex(x => new { x.SchoolId, x.TeacherId, x.CampusId }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasOne<Teacher>().WithMany().HasForeignKey(x => x.TeacherId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
