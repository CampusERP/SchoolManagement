using Application.Common.Interfaces;
using MediatR;
using Application.Common.Exceptions;

namespace Application.Common.Behaviors;

/// <summary>
/// This behavior checks if the request is tenant-scoped and verifies that the current user has access to the specified tenant (school).
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class TenantAuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ITenantContext _tenantContext;

    public TenantAuthorizationBehavior(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is ITenantScopedRequest tenantRequest)
        {
            if (!_tenantContext.IsPlatformAdmin)
            {
                if (_tenantContext.SchoolId is null)
                    throw new ForbiddenException("No school context found in the current session.");

                if (tenantRequest.SchoolId != _tenantContext.SchoolId)
                    throw new ForbiddenException();
            }
        }

        return await next();
    }
}
