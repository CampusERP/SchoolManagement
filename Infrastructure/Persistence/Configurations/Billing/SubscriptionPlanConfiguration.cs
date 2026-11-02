using Domain.Entities.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Billing;

// ── BILLING  (PlatformDbContext) ──────────────────────────────────
public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlans");
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.DescriptionEn).HasMaxLength(500);
        builder.Property(p => p.DescriptionAr).HasMaxLength(500);
        builder.Property(p => p.PriceMonthly).HasPrecision(10, 2);
        builder.HasIndex(p => p.Name).IsUnique();

        builder.HasMany(p => p.Benefits)
            .WithOne()
            .HasForeignKey(b => b.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
