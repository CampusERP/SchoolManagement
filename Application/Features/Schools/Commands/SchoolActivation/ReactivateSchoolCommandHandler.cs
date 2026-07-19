using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Entities.Tenancy;
using MediatR;

namespace Application.Features.Schools.Commands.SchoolActivation;
public class ReactivateSchoolCommandHandler : IRequestHandler<ReactivateSchoolCommand, Result>
{
    private readonly ISchoolRepository _schools;
    public ReactivateSchoolCommandHandler(ISchoolRepository schools) => _schools = schools;
    public async Task<Result> Handle(ReactivateSchoolCommand request, CancellationToken ct)
    {
        var school = await _schools.GetByIdAsync(request.SchoolId, ct);
        if (school is null) throw new NotFoundException(nameof(School), request.SchoolId);
        school.Reactivate();
        return Result.Success();
    }
}