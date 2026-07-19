using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Entities.Tenancy;
using MediatR;

namespace Application.Features.Schools.Commands.SchoolActivation;

public class SuspendSchoolCommandHandler : IRequestHandler<SuspendSchoolCommand, Result>
{
    private readonly ISchoolRepository _schools;
    public SuspendSchoolCommandHandler(ISchoolRepository schools) => _schools = schools;
    public async Task<Result> Handle(SuspendSchoolCommand request, CancellationToken ct)
    {
        var school = await _schools.GetByIdAsync(request.SchoolId, ct);
        if (school is null) throw new NotFoundException(nameof(School), request.SchoolId);
        school.Suspend();
        return Result.Success();
    }
}
