using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Application.Features.People.Queries.GetTeacherById;
using MediatR;

namespace Application.Features.People.Queries.GetMyClasses;

public class GetMyClassesQueryHandler
    : IRequestHandler<GetMyClassesQuery, Result<List<TeachingAssignmentSummaryDto>>>
{
    private readonly ITeacherReadService _teacherReadService;
    private readonly ICurrentUserService _user;

    public GetMyClassesQueryHandler(ITeacherReadService teacherReadService, ICurrentUserService user)
    {
        _teacherReadService = teacherReadService;
        _user = user;
    }

    public async Task<Result<List<TeachingAssignmentSummaryDto>>> Handle(
        GetMyClassesQuery request, CancellationToken ct)
    {
        var userId = _user.UserId;
        if (userId is null) return Result.Failure<List<TeachingAssignmentSummaryDto>>("Not authenticated.");

        var items = await _teacherReadService.GetMyClassesAsync(request.SchoolId, userId.Value, request.TermId, ct);

        return Result.Success(items);
    }
}
