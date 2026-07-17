using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Domain.Entities.Academics;
using Infrastructure.Identity;

namespace Infrastructure.Persistence.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<PlatformDbContext>>();

        try
        {
            var platformDb = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
            var appDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            await platformDb.Database.MigrateAsync();
            await appDb.Database.MigrateAsync();

            await SeedRolesAsync(roleManager, logger);
            await SeedPlatformAdminAsync(userManager, configuration, logger);
            await SeedEducationStagesAsync(appDb, logger);
            await SeedSubjectsAsync(appDb, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager, ILogger logger)
    {
        var roles = new[] { "SuperAdmin", "SchoolAdmin", "Teacher", "Student", "Parent" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                logger.LogInformation("Seeded role: {Role}", role);
            }
        }
    }

    private static async Task SeedPlatformAdminAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger)
    {
        var adminEmail = configuration["Seed:PlatformAdmin:Email"];
        var adminPassword = configuration["Seed:PlatformAdmin:Password"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning("Platform admin seeding skipped because Seed:PlatformAdmin credentials are not configured.");
            return;
        }

        if (await userManager.FindByEmailAsync(adminEmail) is not null) return;

        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            IsPlatformAdmin = true
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "SuperAdmin");
            logger.LogInformation("Seeded platform admin: {Email}", adminEmail);
        }
        else
        {
            logger.LogWarning("Failed to seed platform admin: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private static async Task SeedEducationStagesAsync(ApplicationDbContext db, ILogger logger)
    {
        if (await db.EducationStages.AnyAsync()) return;

        var stages = new[]
        {
            EducationStage.Create("Primary"),
            EducationStage.Create("Middle"),
            EducationStage.Create("Secondary"),
        };

        db.EducationStages.AddRange(stages);
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} education stages.", stages.Length);
    }

    private static async Task SeedSubjectsAsync(ApplicationDbContext db, ILogger logger)
    {
        logger.LogDebug("Subject seeding deferred to Phase 2.");
        await Task.CompletedTask;
    }
}
