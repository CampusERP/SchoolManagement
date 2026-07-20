using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Portal.Queries.GetParentChildAttendance;

public class GetParentChildAttendanceQueryHandler
    : IRequestHandler<GetParentChildAttendanceQuery, Result<PagedResult<PortalAttendanceRecordDto>>>
{
    private readonly IPortalReadService _portalReadService;
    private readonly ICurrentUserService _user;

    public GetParentChildAttendanceQueryHandler(IPortalReadService portalReadService, ICurrentUserService user)
    {
        _portalReadService = portalReadService;
        _user = user;
    }

    public async Task<Result<PagedResult<PortalAttendanceRecordDto>>> Handle(
        GetParentChildAttendanceQuery request, CancellationToken ct)
    {
        var userId = _user.UserId;
        if (userId is null)
            return Result.Failure<PagedResult<PortalAttendanceRecordDto>>("Not authenticated.");

        var p = request.Pagination ?? new PaginationParams();
        var result = await _portalReadService.GetParentChildAttendanceAsync(
            request.SchoolId, userId.Value, request.StudentId, p, ct);

        return Result.Success(result);
    }
}
