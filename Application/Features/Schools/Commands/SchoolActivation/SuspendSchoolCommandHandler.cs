using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Entities.Tenancy;
using MediatR;

namespace Application.Features.Schools.Commands.SchoolActivation;

public class SuspendSchoolCommandHandler : IRequestHandler<SuspendSchoolCommand, Result>
{
    private readonly ISchoolRepository _schools;
    private readonly ITenantContext _tenantContext;

    public SuspendSchoolCommandHandler(ISchoolRepository schools, ITenantContext tenantContext)
    {
        _schools = schools;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(SuspendSchoolCommand request, CancellationToken ct)
    {
        // Suspension locks every account in the school out of the platform.
        // Never let a school-level administrator trigger it for their own or
        // another school, even if a future endpoint forgets the policy.
        if (!_tenantContext.IsPlatformAdmin)
            throw new ForbiddenException("Only a platform administrator can suspend a school.");

        var school = await _schools.GetByIdAsync(request.SchoolId, ct);
        if (school is null) throw new NotFoundException(nameof(School), request.SchoolId);
        school.Suspend();
        return Result.Success();
    }
}
