using Application.Common.Behaviors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Messages;
using Application.Common.Models;
using Application.Common.Services;
using FluentValidation;
using Infrastructure.Authentication;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Interceptors;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Persistence.Services;
using Infrastructure.Outbox;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        var appConn = config.GetConnectionString("ApplicationDb")
            ?? throw new InvalidOperationException(
                "ApplicationDb connection string missing.");

        var platformConn = config.GetConnectionString("PlatformDb")
            ?? throw new InvalidOperationException(
                "PlatformDb connection string missing.");

        // ───────────────── Core ─────────────────

        services.AddHttpContextAccessor();

        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<AuditSaveChangesInterceptor>();

        // ─────────────── Databases ───────────────

        services.AddDbContext<PlatformDbContext>((sp, options) =>
        {
            options.UseSqlServer(platformConn,
                sql => sql.UseQuerySplittingBehavior(
                    QuerySplittingBehavior.SplitQuery));

            options.AddInterceptors(
                sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });


        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseSqlServer(appConn,
                sql => sql.UseQuerySplittingBehavior(
                    QuerySplittingBehavior.SplitQuery));

            options.AddInterceptors(
                sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });


        // ───────────── Unit Of Work ──────────────

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IPlatformUnitOfWork, PlatformUnitOfWork>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPermissionProvider, PermissionProvider>();

        // Repositories
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ITeacherRepository, TeacherRepository>();
        services.AddScoped<IParentRepository, ParentRepository>();
        services.AddScoped<IStudentGuardianRepository, StudentGuardianRepository>();
        services.AddScoped<IAcademicYearRepository, AcademicYearRepository>();
        services.AddScoped<IClassRoomRepository, ClassRoomRepository>();
        services.AddScoped<IGradeLevelRepository, GradeLevelRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IStudentEnrollmentRepository, StudentEnrollmentRepository>();
        services.AddScoped<ITeachingAssignmentRepository, TeachingAssignmentRepository>();
        services.AddScoped<IUserSchoolMembershipRepository, UserSchoolMembershipRepository>();
        services.AddScoped<ISchoolAdminProfileRepository, SchoolAdminProfileRepository>();
        services.AddScoped<ISchoolRepository, SchoolRepository>();
        services.AddScoped<IEducationStageRepository, EducationStageRepository>();

        // Read Services
        services.AddScoped<IStudentReadService, StudentReadService>();
        services.AddScoped<ITeacherReadService, TeacherReadService>();
        services.AddScoped<IParentReadService, ParentReadService>();
        services.AddScoped<ISchoolReadService, SchoolReadService>();
        services.AddScoped<IAcademicReadService, AcademicReadService>();

        services
            .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;

                options.User.RequireUniqueEmail = true;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan =
                    TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<PlatformDbContext>()
            .AddDefaultTokenProviders();

        // ───────────────── JWT ──────────────────

        services.Configure<JwtSettings>(
            config.GetSection("Jwt"));

        var jwtSecret = config["Jwt:Secret"]
            ?? throw new InvalidOperationException(
                "Jwt:Secret not configured.");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = config["Jwt:Issuer"],
                        ValidAudience = config["Jwt:Audience"],

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(jwtSecret)),

                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
            });

        // ───────────── Authorization ─────────────

        services.AddAuthorizationBuilder()

            .AddPolicy("School.Create",
                p => p.RequireClaim(
                    "permissions", "school.create"))

            .AddPolicy("School.Manage",
                p => p.RequireClaim(
                    "permissions", "school.manage"))

            .AddPolicy("School.Read",
                p => p.RequireClaim(
                    "permissions", "school.read"))

            .AddPolicy("Teacher.Create",
                p => p.RequireClaim(
                    "permissions", "teacher.create"))

            .AddPolicy("Teacher.Read",
                p => p.RequireClaim(
                    "permissions", "teacher.read"))

            .AddPolicy("Student.Create",
                p => p.RequireClaim(
                    "permissions", "student.create"))

            .AddPolicy("Student.Read",
                p => p.RequireClaim(
                    "permissions", "student.read"))

            .AddPolicy("Enrollment.Create",
                p => p.RequireClaim(
                    "permissions", "enrollment.create"))

            .AddPolicy("ClassRoom.Create",
                p => p.RequireClaim(
                    "permissions", "classroom.create"))

            .AddPolicy("AcademicYear.Create",
                p => p.RequireClaim(
                    "permissions", "academicyear.create"))

            .AddPolicy("Schedule.Create",
                p => p.RequireClaim(
                    "permissions", "schedule.create"))

            .AddPolicy("Attendance.Record",
                p => p.RequireClaim(
                    "permissions", "attendance.record"))

            .AddPolicy("Grade.Enter",
                p => p.RequireClaim(
                    "permissions", "grade.enter"))

            .AddPolicy("Assignment.Create",
                p => p.RequireClaim(
                    "permissions", "assignment.create"))

            .AddPolicy("Exam.Create",
                p => p.RequireClaim(
                    "permissions", "exam.create"))

            .AddPolicy("Notification.Send",
                p => p.RequireClaim(
                    "permissions", "notification.send"));

        // ───────────── Repositories ─────────────

        services.AddScoped<ISchoolRepository, SchoolRepository>();

        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ITeacherRepository, TeacherRepository>();
        services.AddScoped<IParentRepository, ParentRepository>();
        services.AddScoped<IStudentGuardianRepository, StudentGuardianRepository>();

        services.AddScoped<ISchoolAdminProfileRepository,
            SchoolAdminProfileRepository>();

        services.AddScoped<IUserSchoolMembershipRepository,
            UserSchoolMembershipRepository>();

        services.AddScoped<IAcademicYearRepository,
            AcademicYearRepository>();

        services.AddScoped<IGradeLevelRepository,
            GradeLevelRepository>();

        services.AddScoped<IClassRoomRepository,
            ClassRoomRepository>();

        services.AddScoped<IRoomRepository,
            RoomRepository>();

        // Deferred: these Application contracts have no Infrastructure implementation yet.
        // services.AddScoped<ISubjectQueryRepository, SubjectQueryRepository>();
        // services.AddScoped<ISubjectRepository, SubjectRepository>();
        // services.AddScoped<ICurriculumSubjectRepository, CurriculumSubjectRepository>();
        // services.AddScoped<IAttendanceSessionRepository, AttendanceSessionRepository>();
        // services.AddScoped<IAssignmentRepository, AssignmentRepository>();
        // services.AddScoped<IDocumentRepository, DocumentRepository>();
        // services.AddScoped<IExamRepository, ExamRepository>();
        // services.AddScoped<IExamResultRepository, ExamResultRepository>();
        // services.AddScoped<IReportCardRepository, ReportCardRepository>();
        // services.AddScoped<IDeviceTokenRepository, DeviceTokenRepository>();
        // services.AddScoped<INotificationRepository, NotificationRepository>();
        // services.AddScoped<INotificationBatchRepository, NotificationBatchRepository>();
        // services.AddScoped<IOutboxRepository, OutboxRepository>();
        // services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
        // services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        // services.AddScoped<IInvoiceRepository, InvoiceRepository>();

        services.AddScoped<IStudentEnrollmentRepository,
            StudentEnrollmentRepository>();

        services.AddScoped<ITeachingAssignmentRepository,
            TeachingAssignmentRepository>();

        // ───────────── Application Services ──────

        services.AddScoped<IIdentityService,
            IdentityService>();

        services.AddScoped<IJwtTokenService,
            JwtTokenService>();

        services.AddScoped<IPermissionProvider,
            PermissionProvider>();

        services.AddScoped<IBlobStorageService,
            LocalBlobStorageService>();

        // ───────────── Read Services ─────────────

        services.AddScoped<IStudentReadService,
            StudentReadService>();

        services.AddScoped<ITeacherReadService,
            TeacherReadService>();

        services.AddScoped<IParentReadService,
            ParentReadService>();

        services.AddScoped<ISchoolReadService,
            SchoolReadService>();

        services.AddScoped<IAcademicReadService,
            AcademicReadService>();

        // Deferred read-service implementations:
        // services.AddScoped<IAssignmentReadService, AssignmentReadService>();
        // services.AddScoped<IAttendanceReadService, AttendanceReadService>();
        // services.AddScoped<IBillingReadService, BillingReadService>();
        // services.AddScoped<IExamReadService, ExamReadService>();
        // services.AddScoped<INotificationReadService, NotificationReadService>();
        // services.AddScoped<INotificationRecipientService, NotificationRecipientService>();
        // services.AddScoped<IPortalReadService, PortalReadService>();
        // services.AddScoped<IOutboxService, OutboxService>();
        // services.AddScoped<IEmailService, EmailService>();

        services.AddScoped<IOutboxMessageHandler<CreateSchoolAdminProfileMessage>, CreateSchoolAdminProfileHandler>();
        services.AddScoped<IOutboxMessageHandler<LinkStudentLoginMessage>, LinkStudentLoginHandler>();
        services.AddScoped<IOutboxMessageHandler<LinkTeacherLoginMessage>, LinkTeacherLoginHandler>();
        services.AddScoped<IOutboxMessageHandler<LinkParentLoginMessage>, LinkParentLoginHandler>();
        services.AddScoped<IOutboxMessageHandler<DeliverNotificationBatchMessage>, DeliverNotificationBatchHandler>();
        services.AddHostedService<OutboxProcessor>();

        // ───────────── Outbox Handlers ───────────

        return services;
    }
}
