using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Portal.Queries.GetStudentClasses;

public class GetStudentClassesQueryHandler
    : IRequestHandler<GetStudentClassesQuery, Result<List<StudentClassDto>>>
{
    private readonly IPortalReadService _portalReadService;
    private readonly ICurrentUserService _user;

    public GetStudentClassesQueryHandler(IPortalReadService portalReadService, ICurrentUserService user)
    {
        _portalReadService = portalReadService;
        _user = user;
    }

    public async Task<Result<List<StudentClassDto>>> Handle(
        GetStudentClassesQuery request, CancellationToken ct)
    {
        var userId = _user.UserId;
        if (userId is null)
            return Result.Failure<List<StudentClassDto>>("Not authenticated.");

        var classes = await _portalReadService.GetStudentClassesAsync(
            request.SchoolId, userId.Value, request.StudentEnrollmentId, ct);

        return Result.Success(classes);
    }
}
