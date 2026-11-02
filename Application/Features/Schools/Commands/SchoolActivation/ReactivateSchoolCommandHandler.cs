using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Entities.Tenancy;
using MediatR;

namespace Application.Features.Schools.Commands.SchoolActivation;

public class ReactivateSchoolCommandHandler : IRequestHandler<ReactivateSchoolCommand, Result>
{
    private readonly ISchoolRepository _schools;
    private readonly ITenantContext _tenantContext;

    public ReactivateSchoolCommandHandler(ISchoolRepository schools, ITenantContext tenantContext)
    {
        _schools = schools;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(ReactivateSchoolCommand request, CancellationToken ct)
    {
        // Only a platform administrator may restore a suspended school.
        if (!_tenantContext.IsPlatformAdmin)
            throw new ForbiddenException("Only a platform administrator can reactivate a school.");

        var school = await _schools.GetByIdAsync(request.SchoolId, ct);
        if (school is null) throw new NotFoundException(nameof(School), request.SchoolId);
        school.Reactivate();
        return Result.Success();
    }
}
