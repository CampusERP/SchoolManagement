using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces;
using Domain.Entities.Academics;
using Domain.Entities.Enrollment;
using Domain.Entities.People;

namespace Infrastructure.Persistence;

/// <summary>
/// Holds every tenant-scoped entity. Applies two global query filters to
/// every TenantEntity: (1) SchoolId == current tenant (or bypass for
/// platform admins), and (2) IsDeleted == false. This pair of filters is
/// what makes the shared-database multi-tenancy model safe — do not
/// bypass it with raw SQL
/// </summary>
public class ApplicationDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
        ITenantContext tenantContext) : base(options)
    {
        _tenantContext = tenantContext;
    }

    // People
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<SchoolAdminProfile> SchoolAdminProfiles => Set<SchoolAdminProfile>();
    public DbSet<StudentGuardian> StudentGuardians => Set<StudentGuardian>();

    // Academics
    public DbSet<EducationStage> EducationStages => Set<EducationStage>();
    public DbSet<GradeLevel> GradeLevels => Set<GradeLevel>();
    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<Term> Terms => Set<Term>();
    public DbSet<ClassRoom> ClassRooms => Set<ClassRoom>();
    public DbSet<Room> Rooms => Set<Room>();

    // Enrollment
    public DbSet<StudentEnrollment> StudentEnrollments => Set<StudentEnrollment>();
    public DbSet<TeachingAssignment> TeachingAssignments => Set<TeachingAssignment>();
    public DbSet<ClassSchedule> ClassSchedules => Set<ClassSchedule>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly,
            type => type.Namespace is not null &&
                    (type.Namespace.Contains("Configurations.People") ||
                     type.Namespace.Contains("Configurations.Academics") ||
                     type.Namespace.Contains("Configurations.Enrollment")));

        // TenantEntity-derived types (have SchoolId + IsDeleted)
        builder.Entity<Student>().HasQueryFilter(e =>
            !e.IsDeleted && (_tenantContext.IsPlatformAdmin || e.SchoolId == _tenantContext.SchoolId));
        builder.Entity<Teacher>().HasQueryFilter(e =>
            !e.IsDeleted && (_tenantContext.IsPlatformAdmin || e.SchoolId == _tenantContext.SchoolId));
        builder.Entity<Parent>().HasQueryFilter(e =>
            !e.IsDeleted && (_tenantContext.IsPlatformAdmin || e.SchoolId == _tenantContext.SchoolId));
        builder.Entity<SchoolAdminProfile>().HasQueryFilter(e =>
            !e.IsDeleted && (_tenantContext.IsPlatformAdmin || e.SchoolId == _tenantContext.SchoolId));
        builder.Entity<StudentGuardian>().HasQueryFilter(e =>
            !e.IsDeleted && (_tenantContext.IsPlatformAdmin || e.SchoolId == _tenantContext.SchoolId));

        builder.Entity<GradeLevel>().HasQueryFilter(e =>
            !e.IsDeleted && (_tenantContext.IsPlatformAdmin || e.SchoolId == _tenantContext.SchoolId));
        builder.Entity<AcademicYear>().HasQueryFilter(e =>
            !e.IsDeleted && (_tenantContext.IsPlatformAdmin || e.SchoolId == _tenantContext.SchoolId));
        builder.Entity<ClassRoom>().HasQueryFilter(e =>
            !e.IsDeleted && (_tenantContext.IsPlatformAdmin || e.SchoolId == _tenantContext.SchoolId));
        builder.Entity<Room>().HasQueryFilter(e =>
            !e.IsDeleted && (_tenantContext.IsPlatformAdmin || e.SchoolId == _tenantContext.SchoolId));

        builder.Entity<StudentEnrollment>().HasQueryFilter(e =>
            !e.IsDeleted && (_tenantContext.IsPlatformAdmin || e.SchoolId == _tenantContext.SchoolId));
        builder.Entity<TeachingAssignment>().HasQueryFilter(e =>
            !e.IsDeleted && (_tenantContext.IsPlatformAdmin || e.SchoolId == _tenantContext.SchoolId));

        // AuditableEntity-only types (soft delete, but no SchoolId of their own —
        // they inherit tenant scoping through their parent aggregate instead)
        builder.Entity<Term>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<ClassSchedule>().HasQueryFilter(e => !e.IsDeleted);

        base.OnModelCreating(builder);
    }
}
