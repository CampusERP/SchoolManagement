using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Domain.Entities.Academics;
using Domain.Entities.People;
using Domain.Entities.Tenancy;
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
            await SeedDemoAccountsAsync(platformDb, appDb, userManager, configuration, logger);
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

    private static async Task SeedDemoAccountsAsync(
        PlatformDbContext platformDb,
        ApplicationDbContext appDb,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger)
    {
        var password = configuration["Seed:DemoAccounts:Password"];

        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("Demo account seeding skipped because Seed:DemoAccounts:Password is not configured.");
            return;
        }

        var school = await platformDb.Schools
            .FirstOrDefaultAsync(s => s.SubdomainCode == "demo-school");

        if (school is null)
        {
            school = School.Create("Demo School", "demo-school");
            platformDb.Schools.Add(school);
            await platformDb.SaveChangesAsync();
        }

        _ = await EnsureUserAsync(userManager, "superadmin@demo.school", password, "SuperAdmin", true, logger);
        var schoolAdmin = await EnsureUserAsync(userManager, "schooladmin@demo.school", password, "SchoolAdmin", false, logger);
        var teacher = await EnsureUserAsync(userManager, "teacher@demo.school", password, "Teacher", false, logger);
        var student = await EnsureUserAsync(userManager, "student@demo.school", password, "Student", false, logger);
        var parent = await EnsureUserAsync(userManager, "parent@demo.school", password, "Parent", false, logger);

        foreach (var user in new[] { schoolAdmin, teacher, student, parent }.Where(user => user is not null))
        {
            var hasMembership = await platformDb.UserSchoolMemberships
                .AnyAsync(m => m.ApplicationUserId == user!.Id && m.SchoolId == school.Id);

            if (!hasMembership)
                platformDb.UserSchoolMemberships.Add(UserSchoolMembership.Create(user!.Id, school.Id));
        }

        if (schoolAdmin is not null && !await appDb.SchoolAdminProfiles.IgnoreQueryFilters()
                .AnyAsync(profile => profile.ApplicationUserId == schoolAdmin.Id && profile.SchoolId == school.Id))
        {
            appDb.SchoolAdminProfiles.Add(SchoolAdminProfile.Create(school.Id, schoolAdmin.Id, "School", "Admin"));
        }

        if (teacher is not null && !await appDb.Teachers.IgnoreQueryFilters()
                .AnyAsync(profile => profile.ApplicationUserId == teacher.Id && profile.SchoolId == school.Id))
        {
            appDb.Teachers.Add(Teacher.Create(school.Id, teacher.Id, "DEMO-TEACHER", "Demo", "Teacher"));
        }

        if (student is not null && !await appDb.Students.IgnoreQueryFilters()
                .AnyAsync(profile => profile.ApplicationUserId == student.Id && profile.SchoolId == school.Id))
        {
            var studentProfile = Student.Create(school.Id, "DEMO-STUDENT", "Demo", "Student", new DateTime(2010, 1, 1));
            studentProfile.LinkLogin(student.Id);
            appDb.Students.Add(studentProfile);
        }

        if (parent is not null && !await appDb.Parents.IgnoreQueryFilters()
                .AnyAsync(profile => profile.ApplicationUserId == parent.Id && profile.SchoolId == school.Id))
        {
            appDb.Parents.Add(Parent.Create(school.Id, parent.Id, "Demo", "Parent"));
        }

        await platformDb.SaveChangesAsync();
        await appDb.SaveChangesAsync();

        logger.LogInformation("Seeded demo accounts for every application role.");
    }

    private static async Task<ApplicationUser?> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string role,
        bool isPlatformAdmin,
        ILogger logger)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                IsPlatformAdmin = isPlatformAdmin
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                logger.LogWarning("Failed to seed demo account {Email}: {Errors}",
                    email, string.Join(", ", createResult.Errors.Select(error => error.Description)));
                return null;
            }
        }

        if (isPlatformAdmin && !user.IsPlatformAdmin)
        {
            user.IsPlatformAdmin = true;
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                logger.LogWarning("Failed to mark {Email} as a platform administrator: {Errors}",
                    email, string.Join(", ", updateResult.Errors.Select(error => error.Description)));
                return null;
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                logger.LogWarning("Failed to assign role {Role} to {Email}: {Errors}",
                    role, email, string.Join(", ", roleResult.Errors.Select(error => error.Description)));
                return null;
            }
        }

        return user;
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
