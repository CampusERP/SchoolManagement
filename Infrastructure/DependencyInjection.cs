using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Common.Services;
using Infrastructure.Authentication;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Interceptors;
using Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {

        var platformConnection = configuration.GetConnectionString("PlatformDb")
            ?? throw new InvalidOperationException("Connection string 'PlatformDb' not found.");

        var appConnection = configuration.GetConnectionString("AppDb")
            ?? throw new InvalidOperationException("Connection string 'AppDb' not found.");

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddHttpContextAccessor();

        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<PlatformDbContext>((sp, options) =>
        {
            options.UseSqlServer(platformConnection);
            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseSqlServer(appConnection);
            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });

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

        services
            .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<PlatformDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }
}