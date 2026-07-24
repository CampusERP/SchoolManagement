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
    private static readonly (string Name, string Subdomain, string[] SubjectCodes)[] Schools =
    [
        ("Greenfield Academy", "greenfield", ["MATH", "ENG", "SCI", "COMP", "ART"]),
        ("Riverside International School", "riverside", ["MATH", "ENG", "SCI", "HIST", "GEO"]),
        ("Demo School", "demo-school", ["MATH", "ENG", "SCI", "HIST", "COMP"]),
    ];

    private static readonly (int Seq, string[] Stages)[] GradeBands =
    [
        (9, ["Secondary"]),
        (10, ["Secondary"]),
        (11, ["Secondary"]),
    ];

    private static readonly string[] TeacherFirstNames =
    ["James", "Sarah", "Michael", "Emma", "David", "Olivia", "Daniel", "Sophia", "Robert", "Isabella"];

    private static readonly string[] TeacherLastNames =
    ["Smith", "Johnson", "Brown", "Taylor", "Anderson", "Thomas", "Jackson", "White", "Harris", "Martin"];

    private static readonly string[] StudentFirstNames =
    ["Liam", "Noah", "Oliver", "Elijah", "William", "Henry", "Lucas", "Benjamin", "Theodore", "Jack",
     "Levi", "Alexander", "Mason", "Ethan", "Jacob", "Logan", "Jackson", "Sebastian", "Aiden", "Samuel",
     "Matthew", "Joseph", "David", "Carter", "Owen", "Luke", "Gabriel", "Anthony", "Dylan", "Isaac"];

    private static readonly string[] StudentLastNames =
    ["Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez",
     "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin",
     "Lee", "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson"];

    private static readonly (string[] First, string[] Last) ParentNames =
    (
        ["Robert", "Jennifer", "Michael", "Linda", "William", "Patricia", "Richard", "Barbara", "Joseph",
         "Elizabeth", "Thomas", "Susan", "Charles", "Jessica", "Christopher"],
        ["Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez",
         "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson"]
    );

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

            // Demo data must be explicitly requested.  Running this on every startup
            // can recreate records that were intentionally removed or modified by a
            // user, and the demo data has relationships that only make sense together.
            if (!configuration.GetValue<bool>("Seed:Enabled"))
            {
                logger.LogInformation("Database migrations completed. Demo data seeding is disabled.");
                return;
            }

            var password = configuration["Seed:DemoAccounts:Password"] ?? "Demo@123456";

            await SeedRolesAsync(roleManager, logger);
            await SeedEducationStagesAsync(appDb, logger);
            await SeedSubscriptionPlansAsync(platformDb, logger);
            await SeedSubjectsAsync(appDb, logger);

            foreach (var school in Schools)
                await SeedSchoolAsync(platformDb, appDb, userManager, school, password, logger);

            await platformDb.SaveChangesAsync();
            await appDb.SaveChangesAsync();
            logger.LogInformation("Seeded all schools successfully.");
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

    private static async Task SeedEducationStagesAsync(ApplicationDbContext db, ILogger logger)
    {
        if (await db.EducationStages.AnyAsync()) return;
        db.EducationStages.AddRange(
            EducationStage.Create("Primary"),
            EducationStage.Create("Middle"),
            EducationStage.Create("Secondary"));
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded education stages.");
    }

    private static async Task SeedSubjectsAsync(ApplicationDbContext db, ILogger logger)
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

    private static async Task SeedSubscriptionPlansAsync(PlatformDbContext db, ILogger logger)
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

    // ── Per-school seeding ──────────────────────────────────────────────

    private static async Task SeedSchoolAsync(
        PlatformDbContext platformDb,
        ApplicationDbContext appDb,
        UserManager<ApplicationUser> userManager,
        (string Name, string Subdomain, string[] SubjectCodes) schoolDef,
        string password,
        ILogger logger)
    {
        var school = await platformDb.Schools.FirstOrDefaultAsync(s => s.SubdomainCode == schoolDef.Subdomain);
        if (school is null)
        {
            school = School.Create(schoolDef.Name, schoolDef.Subdomain);
            platformDb.Schools.Add(school);
            await platformDb.SaveChangesAsync();
        }

        var plan = await platformDb.SubscriptionPlans.FirstOrDefaultAsync(p => p.Name == "Standard");
        if (plan is not null && !await platformDb.Subscriptions.AnyAsync(s => s.SchoolId == school.Id))
        {
            var sub = Subscription.Create(school.Id, plan.Id, DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddYears(1));
            sub.GenerateInvoice(49m, DateTime.UtcNow.AddDays(14));
            platformDb.Subscriptions.Add(sub);
        }

        await SeedDemoAccountsAsync(platformDb, appDb, userManager, school, password, logger);
        await SeedAcademicStructureAsync(appDb, school.Id, schoolDef.SubjectCodes, logger);
        await SeedTeachersAsync(platformDb, appDb, userManager, school.Id, schoolDef.Subdomain, password, logger);
        await SeedStudentsAsync(platformDb, appDb, userManager, school.Id, schoolDef.Subdomain, password, logger);
        await SeedParentsAsync(platformDb, appDb, userManager, school.Id, schoolDef.Subdomain, password, logger);
        await SeedAttendanceAsync(appDb, school.Id, logger);
        await SeedAssignmentsAsync(appDb, school.Id, logger);
        await SeedExamsAsync(appDb, school.Id, logger);
        await SeedNotificationsAsync(appDb, school.Id, logger);
    }

    // ── Demo accounts (per school) ──────────────────────────────────────

    private static async Task SeedDemoAccountsAsync(
        PlatformDbContext platformDb,
        ApplicationDbContext appDb,
        UserManager<ApplicationUser> userManager,
        School school,
        string password,
        ILogger logger)
    {
        var superadmin = await EnsureUserAsync(userManager, $"superadmin@{school.SubdomainCode}.school", password, "SuperAdmin", true, logger);
        var schoolAdmin = await EnsureUserAsync(userManager, $"schooladmin@{school.SubdomainCode}.school", password, "SchoolAdmin", false, logger);
        var teacher = await EnsureUserAsync(userManager, $"teacher@{school.SubdomainCode}.school", password, "Teacher", false, logger);
        var student = await EnsureUserAsync(userManager, $"student@{school.SubdomainCode}.school", password, "Student", false, logger);
        var parent = await EnsureUserAsync(userManager, $"parent@{school.SubdomainCode}.school", password, "Parent", false, logger);

        foreach (var user in new[] { superadmin, schoolAdmin, teacher, student, parent }.Where(u => u is not null))
        {
            if (!await platformDb.UserSchoolMemberships.AnyAsync(m => m.ApplicationUserId == user!.Id && m.SchoolId == school.Id))
                platformDb.UserSchoolMemberships.Add(UserSchoolMembership.Create(user!.Id, school.Id, "SuperAdmin"));
        }

        if (schoolAdmin is not null && !await appDb.SchoolAdminProfiles.IgnoreQueryFilters()
                .AnyAsync(p => p.ApplicationUserId == schoolAdmin.Id && p.SchoolId == school.Id))
            appDb.SchoolAdminProfiles.Add(SchoolAdminProfile.Create(school.Id, schoolAdmin.Id, "School", "Admin"));

        if (teacher is not null && !await appDb.Teachers.IgnoreQueryFilters()
                .AnyAsync(t => t.ApplicationUserId == teacher.Id && t.SchoolId == school.Id))
            appDb.Teachers.Add(Teacher.Create(school.Id, teacher.Id, $"DEMO-T-{school.SubdomainCode.ToUpper()}", "Demo", "Teacher"));

        if (student is not null && !await appDb.Students.IgnoreQueryFilters()
                .AnyAsync(s => s.SchoolId == school.Id && s.StudentCode == $"DEMO-S-{school.SubdomainCode.ToUpper()}"))
        {
            var studentProfile = Student.Create(school.Id, $"DEMO-S-{school.SubdomainCode.ToUpper()}", "Demo", "Student", new DateTime(2010, 1, 1));
            studentProfile.LinkLogin(student.Id);
            appDb.Students.Add(studentProfile);
        }

        if (parent is not null && !await appDb.Parents.IgnoreQueryFilters()
                .AnyAsync(p => p.ApplicationUserId == parent.Id && p.SchoolId == school.Id))
            appDb.Parents.Add(Parent.Create(school.Id, parent.Id, "Demo", "Parent"));

        await platformDb.SaveChangesAsync();
        await appDb.SaveChangesAsync();
    }

    // ── Academic structure ──────────────────────────────────────────────

    private static async Task SeedAcademicStructureAsync(
        ApplicationDbContext db, Guid schoolId, string[] subjectCodes, ILogger logger)
    {
        var stages = await db.EducationStages.ToListAsync();
        var secondaryStage = stages.FirstOrDefault(s => s.Name == "Secondary") ?? stages.First();

        var gradeLevels = new List<GradeLevel>();
        foreach (var (seq, stageNames) in GradeBands)
        {
            var stage = stages.FirstOrDefault(s => stageNames.Contains(s.Name)) ?? secondaryStage;
            var existing = await db.GradeLevels.IgnoreQueryFilters()
                .FirstOrDefaultAsync(g => g.SchoolId == schoolId && g.Sequence == seq);
            if (existing is null)
            {
                var grade = GradeLevel.Create(schoolId, stage.Id, $"Grade {seq}", seq);
                db.GradeLevels.Add(grade);
                gradeLevels.Add(grade);
            }
            else
            {
                gradeLevels.Add(existing);
            }
        }
        await db.SaveChangesAsync();

        var year = await GetOrCreateAcademicYearAsync(db, schoolId, logger);

        var brokenClassrooms = await db.ClassRooms.IgnoreQueryFilters()
            .Where(c => c.SchoolId == schoolId && c.AcademicYearId == Guid.Empty)
            .ToListAsync();
        foreach (var c in brokenClassrooms)
        {
            var entry = db.Entry(c);
            entry.Property(nameof(ClassRoom.AcademicYearId)).CurrentValue = year.Id;
        }
        if (brokenClassrooms.Count > 0)
            await db.SaveChangesAsync();

        var classrooms = new List<ClassRoom>();
        var rooms = new List<Room>();
        var suffixes = new[] { "A", "B" };
        foreach (var grade in gradeLevels)
        {
            foreach (var suffix in suffixes)
            {
                var className = $"Grade {grade.Sequence}-{suffix}";
                var existing = await db.ClassRooms.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.SchoolId == schoolId && c.Name == className);
                if (existing is null)
                {
                    var classroom = ClassRoom.Create(schoolId, grade.Id, year.Id, className);
                    db.ClassRooms.Add(classroom);
                    classrooms.Add(classroom);
                }
                else
                {
                    classrooms.Add(existing);
                }
            }
        }

        for (int i = 1; i <= 3; i++)
        {
            var roomName = $"Room {100 + i}";
            if (!await db.Rooms.IgnoreQueryFilters().AnyAsync(r => r.SchoolId == schoolId && r.Name == roomName))
                rooms.Add(Room.Create(schoolId, roomName, 40));
        }
        await db.SaveChangesAsync();

        var curriculumSubjects = new List<CurriculumSubject>();
        foreach (var grade in gradeLevels)
        {
            foreach (var code in subjectCodes)
            {
                var subject = await db.Subjects.FirstOrDefaultAsync(s => s.Code == code);
                if (subject is null) continue;
                if (!await db.CurriculumSubjects.IgnoreQueryFilters()
                        .AnyAsync(cs => cs.SchoolId == schoolId && cs.GradeLevelId == grade.Id && cs.SubjectId == subject.Id))
                {
                    curriculumSubjects.Add(CurriculumSubject.Create(schoolId, grade.Id, subject.Id));
                }
            }
        }
        db.CurriculumSubjects.AddRange(curriculumSubjects);
        await db.SaveChangesAsync();

        logger.LogInformation("Seeded academic structure for school {SchoolId}: {GradeCount} grades, {ClassroomCount} classrooms, {RoomCount} rooms",
            schoolId, gradeLevels.Count, classrooms.Count, rooms.Count);
    }

    private static async Task<AcademicYear> GetOrCreateAcademicYearAsync(
        ApplicationDbContext db, Guid schoolId, ILogger logger)
    {
        var year = await db.AcademicYears.IgnoreQueryFilters()
            .FirstOrDefaultAsync(y => y.SchoolId == schoolId && y.IsCurrent);
        if (year is not null) return year;

        year = AcademicYear.Create(schoolId, "2026/2027", new DateTime(2026, 9, 1), new DateTime(2027, 6, 30));
        year.Activate();
        year.AddTerm("Term 1", 1, new DateTime(2026, 9, 1), new DateTime(2027, 1, 31));
        year.AddTerm("Term 2", 2, new DateTime(2027, 2, 1), new DateTime(2027, 6, 30));
        db.AcademicYears.Add(year);
        await db.SaveChangesAsync();
        logger.LogInformation("Created academic year 2026/2027 for school {SchoolId}.", schoolId);
        return year;
    }

    // ── Teachers ────────────────────────────────────────────────────────

    private static async Task SeedTeachersAsync(
        PlatformDbContext platformDb,
        ApplicationDbContext appDb,
        UserManager<ApplicationUser> userManager,
        Guid schoolId,
        string subdomain,
        string password,
        ILogger logger)
    {
        var year = await appDb.AcademicYears.IgnoreQueryFilters()
            .FirstOrDefaultAsync(y => y.SchoolId == schoolId && y.IsCurrent);
        if (year is null) return;
        var term = await appDb.Terms.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.AcademicYearId == year.Id && t.Sequence == 1);
        if (term is null) return;

        var classrooms = await appDb.ClassRooms.IgnoreQueryFilters().Where(c => c.SchoolId == schoolId).ToListAsync();
        var subjects = await appDb.Subjects.ToListAsync();
        var curriculumSubjects = await appDb.CurriculumSubjects.IgnoreQueryFilters()
            .Where(cs => cs.SchoolId == schoolId).ToListAsync();
        var rooms = await appDb.Rooms.IgnoreQueryFilters().Where(r => r.SchoolId == schoolId).ToListAsync();
        if (classrooms.Count == 0 || subjects.Count == 0 || rooms.Count == 0) return;

        var subjectLookup = subjects.ToDictionary(s => s.Code, s => s.Id);
        var classroomRooms = classrooms.ToDictionary(c => c.Id, c => rooms.First());

        var existingTeachers = await appDb.Teachers.IgnoreQueryFilters()
            .Where(t => t.SchoolId == schoolId)
            .Select(t => t.ApplicationUserId).ToHashSetAsync();

        var users = new List<ApplicationUser>();
        for (int i = 0; i < TeacherFirstNames.Length; i++)
        {
            var email = $"teacher{i + 1}@{subdomain}.school";
            var existing = await userManager.FindByEmailAsync(email);
            if (existing is not null)
            {
                if (!existingTeachers.Contains(existing.Id)) existingTeachers.Add(existing.Id);
                continue;
            }
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                IsPlatformAdmin = false
            };
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Teacher");
                users.Add(user);
                existingTeachers.Add(user.Id);
            }
        }
        await platformDb.SaveChangesAsync();

        var existingTeacherProfiles = await appDb.Teachers.IgnoreQueryFilters()
            .Where(t => t.SchoolId == schoolId).Select(t => t.ApplicationUserId).ToHashSetAsync();

        var teachers = new List<Teacher>();
        int teacherIdx = 0;
        foreach (var userId in existingTeachers.Where(id => !existingTeacherProfiles.Contains(id)).Take(10))
        {
            var idx = teacherIdx % TeacherFirstNames.Length;
            var empCode = $"T-{subdomain.ToUpper().Substring(0, Math.Min(3, subdomain.Length))}-{idx + 1:D3}";
            teachers.Add(Teacher.Create(schoolId, userId, empCode, TeacherFirstNames[idx], TeacherLastNames[idx]));
            teacherIdx++;
        }
        appDb.Teachers.AddRange(teachers);
        await appDb.SaveChangesAsync();

        var allTeachers = await appDb.Teachers.IgnoreQueryFilters()
            .Where(t => t.SchoolId == schoolId)
            .ToListAsync();

        var existingAssignments = await appDb.TeachingAssignments.IgnoreQueryFilters()
            .Where(a => a.SchoolId == schoolId && a.TermId == term.Id)
            .Select(a => new { a.TeacherId, a.SubjectId, a.ClassRoomId })
            .ToHashSetAsync();

        var teachingAssignments = new List<TeachingAssignment>();
        int assignmentIdx = 0;
        foreach (var classroom in classrooms)
        {
            var gradeSubjects = curriculumSubjects
                .Where(cs => cs.GradeLevelId == classroom.GradeLevelId)
                .Select(cs => cs.SubjectId).ToList();
            if (gradeSubjects.Count == 0) continue;

            var subjectId = gradeSubjects[assignmentIdx % gradeSubjects.Count];
            var teacher = allTeachers[assignmentIdx % allTeachers.Count];

            if (!existingAssignments.Contains(new { TeacherId = teacher.Id, SubjectId = subjectId, ClassRoomId = classroom.Id }))
            {
                var ta = TeachingAssignment.Create(schoolId, teacher.Id, subjectId, classroom.Id, term.Id);
                teachingAssignments.Add(ta);
            }
            assignmentIdx++;
        }
        appDb.TeachingAssignments.AddRange(teachingAssignments);
        await appDb.SaveChangesAsync();

        var schedules = new List<ClassSchedule>();
        var times = new (TimeSpan Start, TimeSpan End)[]
        {
            (new TimeSpan(8, 0, 0), new TimeSpan(9, 0, 0)),
            (new TimeSpan(9, 15, 0), new TimeSpan(10, 15, 0)),
            (new TimeSpan(10, 30, 0), new TimeSpan(11, 30, 0)),
            (new TimeSpan(13, 0, 0), new TimeSpan(14, 0, 0)),
            (new TimeSpan(14, 15, 0), new TimeSpan(15, 15, 0)),
        };
        var daySlots = new[] { DayOfWeekEnum.Monday, DayOfWeekEnum.Tuesday, DayOfWeekEnum.Wednesday, DayOfWeekEnum.Thursday, DayOfWeekEnum.Friday };

        var existingSchedules = await appDb.ClassSchedules.IgnoreQueryFilters()
            .Where(cs => teachingAssignments.Select(ta => ta.Id).Contains(cs.TeachingAssignmentId))
            .Select(cs => new { cs.TeachingAssignmentId, cs.DayOfWeek })
            .ToHashSetAsync();

        int scheduleIdx = 0;
        foreach (var ta in teachingAssignments)
        {
            var room = classroomRooms.GetValueOrDefault(ta.ClassRoomId) ?? rooms.First();
            var slot = times[scheduleIdx % times.Length];
            var day = daySlots[scheduleIdx % daySlots.Length];
            if (!existingSchedules.Contains(new { TeachingAssignmentId = ta.Id, DayOfWeek = day }))
            {
                ta.AddSchedule(room.Id, day, slot.Start, slot.End);
            }
            scheduleIdx++;
        }
        await appDb.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} teachers for school {SchoolId}.", allTeachers.Count, schoolId);
    }

    // ── Students ────────────────────────────────────────────────────────

    private static async Task SeedStudentsAsync(
        PlatformDbContext platformDb,
        ApplicationDbContext appDb,
        UserManager<ApplicationUser> userManager,
        Guid schoolId,
        string subdomain,
        string password,
        ILogger logger)
    {
        var year = await appDb.AcademicYears.IgnoreQueryFilters()
            .FirstOrDefaultAsync(y => y.SchoolId == schoolId && y.IsCurrent);
        if (year is null) return;

        var classrooms = await appDb.ClassRooms.IgnoreQueryFilters().Where(c => c.SchoolId == schoolId).ToListAsync();
        if (classrooms.Count == 0) return;

        var existingCodes = await appDb.Students.IgnoreQueryFilters()
            .Where(s => s.SchoolId == schoolId).Select(s => s.StudentCode).ToHashSetAsync();

        var users = new List<ApplicationUser>();
        var students = new List<Student>();
        var enrollments = new List<StudentEnrollment>();
        var userToStudent = new Dictionary<Guid, Student>();
        int created = 0;

        for (int i = 0; i < StudentFirstNames.Length && created < 30; i++)
        {
            var studentCode = $"STU-{subdomain.ToUpper().Substring(0, Math.Min(3, subdomain.Length))}-{created + 1:D3}";
            if (existingCodes.Contains(studentCode)) { created++; continue; }

            var email = $"student{created + 1}@{subdomain}.school";
            var existingUser = await userManager.FindByEmailAsync(email);
            Guid userId;
            if (existingUser is not null)
            {
                userId = existingUser.Id;
            }
            else
            {
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    IsPlatformAdmin = false
                };
                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded) continue;
                await userManager.AddToRoleAsync(user, "Student");
                users.Add(user);
                userId = user.Id;
            }

            var student = Student.Create(schoolId, studentCode, StudentFirstNames[i], StudentLastNames[i % StudentLastNames.Length],
                new DateTime(2012, 1, 1).AddMonths(-created));
            student.LinkLogin(userId);
            students.Add(student);
            userToStudent[userId] = student;
            existingCodes.Add(studentCode);
            created++;
        }
        await platformDb.SaveChangesAsync();
        appDb.Students.AddRange(students);
        await appDb.SaveChangesAsync();

        var existingEnrollments = await appDb.StudentEnrollments.IgnoreQueryFilters()
            .Where(e => e.SchoolId == schoolId && e.AcademicYearId == year.Id)
            .Select(e => e.StudentId).ToHashSetAsync();

        int enrollIdx = 0;
        foreach (var student in students.Where(s => !existingEnrollments.Contains(s.Id)))
        {
            var classroom = classrooms[enrollIdx % classrooms.Count];
            enrollments.Add(StudentEnrollment.Create(schoolId, student.Id, classroom.Id, year.Id));
            enrollIdx++;
        }
        appDb.StudentEnrollments.AddRange(enrollments);
        await appDb.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} students for school {SchoolId}.", students.Count, schoolId);
    }

    // ── Parents ─────────────────────────────────────────────────────────

    private static async Task SeedParentsAsync(
        PlatformDbContext platformDb,
        ApplicationDbContext appDb,
        UserManager<ApplicationUser> userManager,
        Guid schoolId,
        string subdomain,
        string password,
        ILogger logger)
    {
        var students = await appDb.Students.IgnoreQueryFilters()
            .Where(s => s.SchoolId == schoolId && s.ApplicationUserId != null)
            .ToListAsync();
        if (students.Count == 0) return;

        var existingParentUserIds = await appDb.Parents.IgnoreQueryFilters()
            .Where(p => p.SchoolId == schoolId)
            .Select(p => p.ApplicationUserId).ToHashSetAsync();

        var users = new List<ApplicationUser>();
        var parents = new List<Parent>();
        var guardians = new List<StudentGuardian>();
        int created = 0;

        for (int i = 0; i < ParentNames.First.Length && created < 15; i++)
        {
            var email = $"parent{created + 1}@{subdomain}.school";
            var existingUser = await userManager.FindByEmailAsync(email);
            Guid userId;
            if (existingUser is not null)
            {
                userId = existingUser.Id;
                if (existingParentUserIds.Contains(userId)) { created++; continue; }
            }
            else
            {
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    IsPlatformAdmin = false
                };
                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded) continue;
                await userManager.AddToRoleAsync(user, "Parent");
                users.Add(user);
                userId = user.Id;
            }

            if (!existingParentUserIds.Contains(userId))
            {
                var parent = Parent.Create(schoolId, userId, ParentNames.First[i], ParentNames.Last[i]);
                parents.Add(parent);
                existingParentUserIds.Add(userId);
            }
            created++;
        }
        await platformDb.SaveChangesAsync();
        appDb.Parents.AddRange(parents);
        await appDb.SaveChangesAsync();

        var allParents = await appDb.Parents.IgnoreQueryFilters()
            .Where(p => p.SchoolId == schoolId).ToListAsync();

        var existingGuardians = await appDb.StudentGuardians.IgnoreQueryFilters()
            .Where(g => g.SchoolId == schoolId)
            .Select(g => new { g.StudentId, g.ParentId }).ToHashSetAsync();

        int parentIdx = 0;
        foreach (var student in students)
        {
            var parent = allParents[parentIdx % allParents.Count];
            if (!existingGuardians.Contains(new { StudentId = student.Id, ParentId = parent.Id }))
            {
                guardians.Add(StudentGuardian.Create(schoolId, student.Id, parent.Id,
                    GuardianRelationshipType.Father, parentIdx == 0, true, false));
            }
            parentIdx++;
        }
        appDb.StudentGuardians.AddRange(guardians);
        await appDb.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} parents for school {SchoolId}.", parents.Count, schoolId);
    }

    // ── Attendance ──────────────────────────────────────────────────────

    private static async Task SeedAttendanceAsync(ApplicationDbContext db, Guid schoolId, ILogger logger)
    {
        var taIds = await db.TeachingAssignments.IgnoreQueryFilters()
            .Where(ta => ta.SchoolId == schoolId)
            .Select(ta => ta.Id).ToListAsync();
        var schedules = await db.ClassSchedules.IgnoreQueryFilters()
            .Where(cs => taIds.Contains(cs.TeachingAssignmentId))
            .ToListAsync();
        if (schedules.Count == 0) return;

        var enrollments = await db.StudentEnrollments.IgnoreQueryFilters()
            .Where(e => e.SchoolId == schoolId).ToListAsync();
        if (enrollments.Count == 0) return;

        var existingKeys = await db.AttendanceSessions.IgnoreQueryFilters()
            .Where(s => s.SchoolId == schoolId)
            .Select(s => new { s.ClassScheduleId, s.Date })
            .ToHashSetAsync();

        var sessions = new List<AttendanceSession>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        for (int d = 1; d <= 14; d++)
        {
            var date = today.AddDays(-d);
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) continue;
            foreach (var schedule in schedules)
            {
                if (!existingKeys.Contains(new { ClassScheduleId = schedule.Id, Date = date }))
                {
                    var session = AttendanceSession.Open(schoolId, schedule.Id, date);
                    foreach (var enrollment in enrollments)
                        session.RecordStudent(enrollment.Id, AttendanceStatus.Present);
                    sessions.Add(session);
                }
            }
        }
        db.AttendanceSessions.AddRange(sessions);
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} attendance sessions for school {SchoolId}.", sessions.Count, schoolId);
    }

    // ── Assignments ─────────────────────────────────────────────────────

    private static async Task SeedAssignmentsAsync(ApplicationDbContext db, Guid schoolId, ILogger logger)
    {
        var teachingAssignments = await db.TeachingAssignments.IgnoreQueryFilters()
            .Where(ta => ta.SchoolId == schoolId).ToListAsync();
        if (teachingAssignments.Count == 0) return;

        var taIds = teachingAssignments.Select(ta => ta.Id).ToList();
        var existingTitles = await db.Assignments.IgnoreQueryFilters()
            .Where(a => taIds.Contains(a.TeachingAssignmentId))
            .Select(a => a.Title).ToHashSetAsync();

        var assignments = new List<Assignment>();
        string[] titles = ["Homework 1", "Quiz 1", "Classwork 1", "Practice Problems", "Research Paper"];
        int idx = 0;
        foreach (var ta in teachingAssignments)
        {
            var title = titles[idx % titles.Length];
            if (!existingTitles.Contains(title))
            {
                assignments.Add(Assignment.Create(schoolId, ta.Id, title,
                    $"Complete the tasks for {title}.", DateTime.UtcNow.AddDays(7), 20));
                existingTitles.Add(title);
            }
            idx++;
        }
        db.Assignments.AddRange(assignments);
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} assignments for school {SchoolId}.", assignments.Count, schoolId);
    }

    // ── Exams ───────────────────────────────────────────────────────────

    private static async Task SeedExamsAsync(ApplicationDbContext db, Guid schoolId, ILogger logger)
    {
        var year = await db.AcademicYears.IgnoreQueryFilters()
            .FirstOrDefaultAsync(y => y.SchoolId == schoolId && y.IsCurrent);
        if (year is null) return;
        var term = await db.Terms.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.AcademicYearId == year.Id && t.Sequence == 1);
        if (term is null) return;

        var subjects = await db.Subjects.ToListAsync();
        var classrooms = await db.ClassRooms.IgnoreQueryFilters().Where(c => c.SchoolId == schoolId).ToListAsync();
        var rooms = await db.Rooms.IgnoreQueryFilters().Where(r => r.SchoolId == schoolId).ToListAsync();
        var enrollments = await db.StudentEnrollments.IgnoreQueryFilters()
            .Where(e => e.SchoolId == schoolId && e.AcademicYearId == year.Id).ToListAsync();
        if (subjects.Count == 0 || classrooms.Count == 0 || rooms.Count == 0) return;

        var existingExamNames = await db.Exams.IgnoreQueryFilters()
            .Where(e => e.SchoolId == schoolId && e.TermId == term.Id)
            .Select(e => e.Name).ToHashSetAsync();

        var exams = new List<Exam>();
        string[] examNames = ["Midterm Examination", "Final Examination", "Class Quiz"];
        int idx = 0;
        foreach (var subject in subjects.Take(3))
        {
            var examName = examNames[idx % examNames.Length];
            if (!existingExamNames.Contains(examName))
            {
                var exam = Exam.Create(schoolId, subject.Id, term.Id, examName, 100);
                var classroom = classrooms[idx % classrooms.Count];
                var room = rooms.First();
                var schedule = exam.AddSchedule(classroom.Id, room.Id, DateTime.UtcNow.AddDays(-5));
                foreach (var enrollment in enrollments.Where(e => e.ClassRoomId == classroom.Id))
                    exam.RecordResult(schedule.Id, enrollment.Id, Random.Shared.Next(50, 100));
                exam.Lock();
                exams.Add(exam);
                existingExamNames.Add(examName);
            }
            idx++;
        }
        db.Exams.AddRange(exams);
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} exams for school {SchoolId}.", exams.Count, schoolId);
    }

    // ── Notifications ───────────────────────────────────────────────────

    private static async Task SeedNotificationsAsync(ApplicationDbContext db, Guid schoolId, ILogger logger)
    {
        if (await db.NotificationBatches.IgnoreQueryFilters().AnyAsync(b => b.SchoolId == schoolId)) return;

        var teacher = await db.Teachers.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.SchoolId == schoolId);
        if (teacher is null) return;

        var student = await db.Students.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.SchoolId == schoolId && s.ApplicationUserId != null);
        var parent = await db.Parents.IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.SchoolId == schoolId);

        var batch = NotificationBatch.Create(schoolId, null, teacher.ApplicationUserId,
            "Welcome to the portal", "Your timetable, attendance, assignments, and grades are ready to view.",
            NotificationChannel.InApp, "All users");

        if (student?.ApplicationUserId is Guid studentUserId) batch.AddRecipient(studentUserId).MarkDelivered();
        batch.AddRecipient(parent.ApplicationUserId).MarkDelivered();
        db.NotificationBatches.Add(batch);
        await db.SaveChangesAsync();
    }

    // ── Helpers ─────────────────────────────────────────────────────────

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
                logger.LogWarning("Failed to seed {Email}: {Errors}",
                    email, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                return null;
            }
        }

        if (isPlatformAdmin && !user.IsPlatformAdmin)
        {
            user.IsPlatformAdmin = true;
            await userManager.UpdateAsync(user);
        }

        if (!await userManager.IsInRoleAsync(user, role))
            await userManager.AddToRoleAsync(user, role);

        return user;
    }
}
