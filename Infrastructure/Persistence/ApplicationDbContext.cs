using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities.Academics;
using Domain.Entities.Assignments;
using Domain.Entities.Attendance;
using Domain.Entities.Documents;
using Domain.Entities.Enrollment;
using Domain.Entities.Exams;
using Domain.Entities.Notifications;
using Domain.Entities.People;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    // ───────────────────── People ─────────────────────

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<SchoolAdminProfile> SchoolAdminProfiles => Set<SchoolAdminProfile>();
    public DbSet<StudentGuardian> StudentGuardians => Set<StudentGuardian>();

    // ──────────────────── Academics ───────────────────

    public DbSet<EducationStage> EducationStages => Set<EducationStage>();
    public DbSet<GradeLevel> GradeLevels => Set<GradeLevel>();
    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<Term> Terms => Set<Term>();
    public DbSet<ClassRoom> ClassRooms => Set<ClassRoom>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<CurriculumSubject> CurriculumSubjects => Set<CurriculumSubject>();

    // ─────────────────── Enrollment ───────────────────

    public DbSet<StudentEnrollment> StudentEnrollments => Set<StudentEnrollment>();
    public DbSet<TeachingAssignment> TeachingAssignments => Set<TeachingAssignment>();
    public DbSet<ClassSchedule> ClassSchedules => Set<ClassSchedule>();

    // ─────────────────── Attendance ───────────────────

    public DbSet<AttendanceSession> AttendanceSessions => Set<AttendanceSession>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();

    // ─────────────────── Assignments ──────────────────

    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<AssignmentSubmission> AssignmentSubmissions => Set<AssignmentSubmission>();
    public DbSet<AssignmentSubmissionDocument> AssignmentSubmissionDocuments => Set<AssignmentSubmissionDocument>();

    // ───────────────────── Documents ──────────────────

    public DbSet<Document> Documents => Set<Document>();

    // ─────────────────────── Exams ────────────────────

    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<ExamSchedule> ExamSchedules => Set<ExamSchedule>();
    public DbSet<ExamResult> ExamResults => Set<ExamResult>();
    public DbSet<ReportCard> ReportCards => Set<ReportCard>();
    public DbSet<ReportCardSubjectResult> ReportCardSubjectResults => Set<ReportCardSubjectResult>();

    // ─────────────────── Notifications ────────────────

    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<NotificationBatch> NotificationBatches => Set<NotificationBatch>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        ApplyTenantFilter<Student>(builder);
        ApplyTenantFilter<Teacher>(builder);
        ApplyTenantFilter<Parent>(builder);
        ApplyTenantFilter<SchoolAdminProfile>(builder);
        ApplyTenantFilter<StudentGuardian>(builder);

        ApplyTenantFilter<GradeLevel>(builder);
        ApplyTenantFilter<AcademicYear>(builder);
        ApplyTenantFilter<ClassRoom>(builder);
        ApplyTenantFilter<Room>(builder);
        ApplyTenantFilter<CurriculumSubject>(builder);

        ApplyTenantFilter<StudentEnrollment>(builder);
        ApplyTenantFilter<TeachingAssignment>(builder);

        ApplyTenantFilter<AttendanceSession>(builder);

        ApplyTenantFilter<Assignment>(builder);

        ApplyTenantFilter<Document>(builder);

        ApplyTenantFilter<Exam>(builder);
        ApplyTenantFilter<ReportCard>(builder);

        ApplyTenantFilter<NotificationTemplate>(builder);
        ApplyTenantFilter<NotificationBatch>(builder);

        ApplySoftDeleteFilter<Term>(builder);
        ApplySoftDeleteFilter<ClassSchedule>(builder);
        ApplySoftDeleteFilter<AttendanceRecord>(builder);
        ApplySoftDeleteFilter<AssignmentSubmission>(builder);
        ApplySoftDeleteFilter<AssignmentSubmissionDocument>(builder);
        ApplySoftDeleteFilter<ExamResult>(builder);
        ApplySoftDeleteFilter<ReportCardSubjectResult>(builder);
        ApplySoftDeleteFilter<Notification>(builder);

        base.OnModelCreating(builder);
    }

    private void ApplyTenantFilter<T>(ModelBuilder builder)
        where T : TenantEntity
    {
        builder.Entity<T>().HasQueryFilter(e =>
            !e.IsDeleted &&
            (_tenantContext.IsPlatformAdmin ||
             e.SchoolId == _tenantContext.SchoolId));
    }

    private static void ApplySoftDeleteFilter<T>(ModelBuilder builder)
        where T : AuditableEntity
    {
        builder.Entity<T>()
            .HasQueryFilter(e => !e.IsDeleted);
    }
}
