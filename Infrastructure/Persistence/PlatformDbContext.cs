using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Tenancy;
using Infrastructure.Identity;

namespace Infrastructure.Persistence;

/// <summary>
/// Represents the database context for the platform, including Identity and tenancy-related entities.
/// </summary>
public class PlatformDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public PlatformDbContext(DbContextOptions<PlatformDbContext> options) : base(options) { }

    public DbSet<School> Schools => Set<School>();
    public DbSet<Campus> Campuses => Set<Campus>();
    public DbSet<UserSchoolMembership> UserSchoolMemberships => Set<UserSchoolMembership>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // keep Identity's own table mappings

        builder.ApplyConfigurationsFromAssembly(
            typeof(PlatformDbContext).Assembly,
            type => type.Namespace is not null &&
                    type.Namespace.Contains("Configurations.Tenancy"));
    }
}
