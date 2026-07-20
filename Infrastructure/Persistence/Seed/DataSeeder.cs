using Domain.Entities.Academics;
using Domain.Entities.Billing;
using Domain.Entities.People;
using Domain.Entities.Tenancy;
using Domain.Entities.Enrollment;
using Domain.Entities.Assignments;
using Domain.Entities.Attendance;
using Domain.Entities.Exams;
using Domain.Entities.Notifications;
using Domain.Enums;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            await SeedSubscriptionPlansAsync(platformDb, logger);
            await SeedDemoBillingAsync(platformDb, logger);
            await SeedSubjectsAsync(appDb, logger);
            await SeedDemoSchoolDataAsync(platformDb, appDb, logger);
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
                platformDb.UserSchoolMemberships.Add(UserSchoolMembership.Create(user!.Id, school.Id, "SuperAdmin"));
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

    private static async Task SeedSubjectsAsync(
        ApplicationDbContext db, ILogger logger)
    {
        if (await db.Subjects.AnyAsync()) return;

        db.Subjects.AddRange(
            Subject.Create("MATH", "Mathematics"),
            Subject.Create("ENG", "English Language"),
            Subject.Create("SCI", "Science"),
            Subject.Create("HIST", "History"),
            Subject.Create("GEO", "Geography"),
            Subject.Create("ART", "Art"),
            Subject.Create("PE", "Physical Education"),
            Subject.Create("COMP", "Computer Science"),
            Subject.Create("RELI", "Religious Studies"),
            Subject.Create("MUSIC", "Music"),
            Subject.Create("ARAB", "Arabic Language"),
            Subject.Create("FREN", "French Language"),
            Subject.Create("BIO", "Biology"),
            Subject.Create("CHEM", "Chemistry"),
            Subject.Create("PHYS", "Physics"));

        await db.SaveChangesAsync();
        logger.LogInformation("Seeded subjects.");
    }

    private static async Task SeedSubscriptionPlansAsync(
        PlatformDbContext db, ILogger logger)
    {
        if (await db.SubscriptionPlans.AnyAsync()) return;

        db.SubscriptionPlans.AddRange(
            SubscriptionPlan.Create("Free", 0, 100, 10,
                parentPortal: false, examModule: false, analytics: false),
            SubscriptionPlan.Create("Standard", 49, 500, 50,
                parentPortal: true, examModule: true, analytics: false),
            SubscriptionPlan.Create("Premium", 99, 2000, 200,
                parentPortal: true, examModule: true, analytics: true));

        await db.SaveChangesAsync();
        logger.LogInformation("Seeded subscription plans.");
    }

    private static async Task SeedDemoBillingAsync(PlatformDbContext db, ILogger logger)
    {
        var school = await db.Schools.FirstOrDefaultAsync(s => s.SubdomainCode == "demo-school");
        var plan = await db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Name == "Standard");
        if (school is null || plan is null) return;

        var subscription = await db.Subscriptions.FirstOrDefaultAsync(s => s.SchoolId == school.Id);
        if (subscription is null)
        {
            subscription = Subscription.Create(school.Id, plan.Id, DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddYears(1));
            subscription.GenerateInvoice(49m, DateTime.UtcNow.AddDays(14));
            db.Subscriptions.Add(subscription);
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded demo subscription and invoice.");
        }
    }

    /// <summary>Creates the minimum connected academic graph needed by the demo accounts and portals.</summary>
    private static async Task SeedDemoSchoolDataAsync(
        PlatformDbContext platformDb, ApplicationDbContext db, ILogger logger)
    {
        var school = await platformDb.Schools.FirstOrDefaultAsync(s => s.SubdomainCode == "demo-school");
        if (school is null) return;

        var stage = await db.EducationStages.FirstAsync(s => s.Name == "Secondary");
        var year = await db.AcademicYears.IgnoreQueryFilters()
            .FirstOrDefaultAsync(y => y.SchoolId == school.Id && y.IsCurrent);
        if (year is null)
        {
            year = AcademicYear.Create(school.Id, "2026/2027", new DateTime(2026, 9, 1), new DateTime(2027, 6, 30));
            year.Activate();
            year.AddTerm("Term 1", 1, new DateTime(2026, 9, 1), new DateTime(2027, 1, 31));
            year.AddTerm("Term 2", 2, new DateTime(2027, 2, 1), new DateTime(2027, 6, 30));
            db.AcademicYears.Add(year);
            await db.SaveChangesAsync();
        }

        var grade = await db.GradeLevels.IgnoreQueryFilters().FirstOrDefaultAsync(g => g.SchoolId == school.Id && g.Name == "Grade 10");
        if (grade is null) { grade = GradeLevel.Create(school.Id, stage.Id, "Grade 10", 10); db.GradeLevels.Add(grade); await db.SaveChangesAsync(); }
        var room = await db.Rooms.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.SchoolId == school.Id && r.Name == "Room 101");
        if (room is null) { room = Room.Create(school.Id, "Room 101", 35); db.Rooms.Add(room); await db.SaveChangesAsync(); }
        var classroom = await db.ClassRooms.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.SchoolId == school.Id && c.Name == "Grade 10-A");
        if (classroom is null) { classroom = ClassRoom.Create(school.Id, grade.Id, year.Id, "Grade 10-A"); db.ClassRooms.Add(classroom); await db.SaveChangesAsync(); }

        var student = await db.Students.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.SchoolId == school.Id && s.StudentCode == "DEMO-STUDENT");
        var parent = await db.Parents.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.SchoolId == school.Id);
        var teacher = await db.Teachers.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.SchoolId == school.Id && t.EmployeeCode == "DEMO-TEACHER");
        if (student is null || parent is null || teacher is null) return;

        var enrollment = await db.StudentEnrollments.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.StudentId == student.Id && e.AcademicYearId == year.Id);
        if (enrollment is null) { enrollment = StudentEnrollment.Create(school.Id, student.Id, classroom.Id, year.Id); db.StudentEnrollments.Add(enrollment); await db.SaveChangesAsync(); }
        if (!await db.StudentGuardians.IgnoreQueryFilters().AnyAsync(g => g.StudentId == student.Id && g.ParentId == parent.Id))
        { db.StudentGuardians.Add(StudentGuardian.Create(school.Id, student.Id, parent.Id, GuardianRelationshipType.Father, true, true, true)); await db.SaveChangesAsync(); }

        var term = await db.Terms.IgnoreQueryFilters().FirstAsync(t => t.AcademicYearId == year.Id && t.Sequence == 1);
        var math = await db.Subjects.FirstAsync(s => s.Code == "MATH");
        if (!await db.TeachingAssignments.IgnoreQueryFilters().AnyAsync(a => a.TeacherId == teacher.Id && a.SubjectId == math.Id && a.ClassRoomId == classroom.Id && a.TermId == term.Id))
        {
            var assignment = TeachingAssignment.Create(school.Id, teacher.Id, math.Id, classroom.Id, term.Id);
            assignment.AddSchedule(room.Id, DayOfWeekEnum.Monday, new TimeSpan(8, 0, 0), new TimeSpan(9, 0, 0));
            assignment.AddSchedule(room.Id, DayOfWeekEnum.Wednesday, new TimeSpan(8, 0, 0), new TimeSpan(9, 0, 0));
            db.TeachingAssignments.Add(assignment);
            await db.SaveChangesAsync();
        }

        var teaching = await db.TeachingAssignments.IgnoreQueryFilters().FirstAsync(a => a.TeacherId == teacher.Id && a.SubjectId == math.Id && a.ClassRoomId == classroom.Id && a.TermId == term.Id);
        if (!await db.Assignments.IgnoreQueryFilters().AnyAsync(a => a.TeachingAssignmentId == teaching.Id))
        {
            var homework = Assignment.Create(school.Id, teaching.Id, "Algebra practice", "Complete questions 1–10 before the next lesson.", DateTime.UtcNow.AddDays(5), 20);
            db.Assignments.Add(homework);
        }

        var schedule = await db.ClassSchedules.FirstAsync(s => s.TeachingAssignmentId == teaching.Id);
        if (!await db.AttendanceSessions.IgnoreQueryFilters().AnyAsync(s => s.ClassScheduleId == schedule.Id))
        {
            var session = AttendanceSession.Open(school.Id, schedule.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)));
            session.RecordStudent(enrollment.Id, AttendanceStatus.Present);
            session.Lock();
            db.AttendanceSessions.Add(session);
        }

        if (!await db.Exams.IgnoreQueryFilters().AnyAsync(e => e.SubjectId == math.Id && e.TermId == term.Id))
        {
            var exam = Exam.Create(school.Id, math.Id, term.Id, "Mathematics Midterm", 100);
            var examSchedule = exam.AddSchedule(classroom.Id, room.Id, DateTime.UtcNow.AddDays(-10));
            exam.RecordResult(examSchedule.Id, enrollment.Id, 88);
            exam.Lock();
            db.Exams.Add(exam);

            var card = ReportCard.Generate(school.Id, enrollment.Id, term.Id, 88, "B+", teacher.ApplicationUserId,
                [(math.Id, math.Name, 88m, 100m, "B+")]);
            card.Lock();
            db.ReportCards.Add(card);
        }

        if (!await db.NotificationBatches.IgnoreQueryFilters().AnyAsync(b => b.SchoolId == school.Id && b.Subject == "Welcome to the demo portal"))
        {
            var batch = NotificationBatch.Create(school.Id, null, teacher.ApplicationUserId,
                "Welcome to the demo portal", "Your timetable, attendance, assignments, and grades are ready to view.", NotificationChannel.InApp, "Demo users");
            batch.AddRecipient(student.ApplicationUserId!.Value).MarkDelivered();
            batch.AddRecipient(parent.ApplicationUserId).MarkDelivered();
            db.NotificationBatches.Add(batch);
        }

        await db.SaveChangesAsync();

        logger.LogInformation("Seeded connected demo school data for the student and parent portals.");
    }
}
