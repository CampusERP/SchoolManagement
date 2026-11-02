using Domain.Entities.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Billing;

public class SubscriptionPlanBenefitConfiguration : IEntityTypeConfiguration<SubscriptionPlanBenefit>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlanBenefit> builder)
    {
        builder.ToTable("SubscriptionPlanBenefits");
        builder.Property(b => b.DescriptionEn).HasMaxLength(500);
        builder.Property(b => b.DescriptionAr).HasMaxLength(500);
        builder.HasIndex(b => b.SubscriptionPlanId);
    }
}
