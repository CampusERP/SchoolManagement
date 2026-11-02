using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Schools.Commands.UpdateCampus;

public sealed class UpdateCampusCommandHandler : IRequestHandler<UpdateCampusCommand, Result>
{
    private readonly ISchoolRepository _schools;
    public UpdateCampusCommandHandler(ISchoolRepository schools) => _schools = schools;

    public async Task<Result> Handle(UpdateCampusCommand request, CancellationToken ct)
    {
        var campus = await _schools.GetCampusAsync(request.SchoolId, request.CampusId, ct);
        if (campus is null) return Result.Failure("Campus not found.");
        campus.Update(request.Name, request.Address);
        return Result.Success();
    }
}
