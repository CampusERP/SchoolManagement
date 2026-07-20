using Domain.Entities.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Billing;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(s => s.SchoolId);
        // Filtered: one active subscription per school
        builder.HasIndex(s => s.SchoolId)
            .HasDatabaseName("IX_Subscriptions_OneActivePerSchool")
            .IsUnique()
            .HasFilter("[Status] = 'Active'");
        builder.HasMany(s => s.Invoices)
            .WithOne().HasForeignKey(i => i.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(s => s.RowVersion).IsRowVersion();
    }
}
