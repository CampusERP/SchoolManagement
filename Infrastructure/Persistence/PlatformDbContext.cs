using Domain.Entities.Billing;
using Domain.Entities.Outbox;
using Domain.Entities.Tenancy;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// Represents the database context for the platform, including Identity and tenancy-related entities.
/// </summary>
public class PlatformDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public PlatformDbContext(DbContextOptions<PlatformDbContext> options) : base(options) { }

    // ─────────────── Tenancy ───────────────

    public DbSet<School> Schools => Set<School>();
    public DbSet<Campus> Campuses => Set<Campus>();
    public DbSet<UserSchoolMembership> UserSchoolMemberships => Set<UserSchoolMembership>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // ─────────────── Billing ───────────────

    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Payment> Payments => Set<Payment>();

    // ─────────────── Outbox ───────────────

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(
            typeof(PlatformDbContext).Assembly,
            t => t.Namespace != null && (
                t.Namespace.Contains("Configurations.Tenancy") ||
                t.Namespace.Contains("Configurations.Billing") ||
                t.Namespace.Contains("Configurations.Outbox")));
    }
}