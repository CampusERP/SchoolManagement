using Domain.Entities.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Tenancy;

public class SchoolRegistrationRequestConfiguration : IEntityTypeConfiguration<SchoolRegistrationRequest>
{
    public void Configure(EntityTypeBuilder<SchoolRegistrationRequest> builder)
    {
        builder.ToTable("SchoolRegistrationRequests");

        builder.Property(r => r.SchoolName).IsRequired().HasMaxLength(200);
        builder.Property(r => r.SubdomainCode).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Address).HasMaxLength(500);
        builder.Property(r => r.Phone).HasMaxLength(50);
        builder.Property(r => r.SchoolEmail).HasMaxLength(256);

        builder.Property(r => r.AdminFirstName).IsRequired().HasMaxLength(100);
        builder.Property(r => r.AdminLastName).IsRequired().HasMaxLength(100);
        builder.Property(r => r.AdminEmail).IsRequired().HasMaxLength(256);
        builder.Property(r => r.AdminPassword).IsRequired().HasMaxLength(256);

        builder.Property(r => r.Status).IsRequired().HasMaxLength(50);
        builder.Property(r => r.ReviewNotes).HasMaxLength(1000);

        builder.Property(r => r.PlanPriceMonthly).HasPrecision(18, 2);

        builder.Property(r => r.RowVersion).IsRowVersion();

        // A school cannot be registered twice with the same desired subdomain while a
        // request is still pending. (Approved schools are guarded by the Schools unique index.)
        builder.HasIndex(r => r.SubdomainCode);
        builder.HasIndex(r => r.Status);
    }
}
