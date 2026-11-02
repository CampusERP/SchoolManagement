using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Schools.Commands.DeleteCampus;

public sealed class DeleteCampusCommandHandler : IRequestHandler<DeleteCampusCommand, Result>
{
    private readonly ISchoolRepository _schools;
    public DeleteCampusCommandHandler(ISchoolRepository schools) => _schools = schools;

    public async Task<Result> Handle(DeleteCampusCommand request, CancellationToken ct)
    {
        var campus = await _schools.GetCampusAsync(request.SchoolId, request.CampusId, ct);
        if (campus is null) return Result.Failure("Campus not found.");
        await _schools.DeleteCampusAsync(campus, ct);
        return Result.Success();
    }
}
