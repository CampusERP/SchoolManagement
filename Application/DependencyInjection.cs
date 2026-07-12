using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Application.Common.Behaviors;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // MediatR scans the Application assembly for all IRequestHandler<,> implementations.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // FluentValidation scans the same assembly for all AbstractValidator<T> implementations.
        services.AddValidatorsFromAssembly(assembly);

        // Pipeline behaviors run in this exact order for every MediatR request:
        // 1. Logging     — logs request name + duration, flags slow handlers
        // 2. Validation  — runs all FluentValidation validators; throws on failure
        // 3. TenantAuth  — verifies request.SchoolId matches the caller's JWT school_id
        // 4. Transaction — calls SaveChangesAsync after commands; skips for queries
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TenantAuthorizationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AppTransactionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PlatformTransactionBehavior<,>));

        return services;
    }
}
