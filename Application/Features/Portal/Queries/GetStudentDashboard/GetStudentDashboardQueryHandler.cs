using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Portal.Queries.GetStudentDashboard;

public class GetStudentDashboardQueryHandler
    : IRequestHandler<GetStudentDashboardQuery, Result<StudentDashboardDto>>
{
    private readonly IPortalReadService _portalReadService;
    private readonly ICurrentUserService _user;

    public GetStudentDashboardQueryHandler(IPortalReadService portalReadService, ICurrentUserService user)
    {
        _portalReadService = portalReadService;
        _user = user;
    }

    public async Task<Result<StudentDashboardDto>> Handle(
        GetStudentDashboardQuery request, CancellationToken ct)
    {
        var userId = _user.UserId;
        if (userId is null)
            return Result.Failure<StudentDashboardDto>("Not authenticated.");

        var dashboard = await _portalReadService.GetStudentDashboardAsync(
            request.SchoolId, userId.Value, request.StudentEnrollmentId, ct);

        if (dashboard is null)
            return Result.Failure<StudentDashboardDto>("Enrollment not found.");

        return Result.Success(dashboard);
    }
}
