using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Application.Features.Portal.Queries.Shared;
using MediatR;

namespace Application.Features.Portal.Queries.GetParentChildAssignments;

public class GetParentChildAssignmentsQueryHandler
    : IRequestHandler<GetParentChildAssignmentsQuery, Result<PagedResult<PortalAssignmentDto>>>
{
    private readonly IPortalReadService _portalReadService;
    private readonly ICurrentUserService _user;

    public GetParentChildAssignmentsQueryHandler(IPortalReadService portalReadService, ICurrentUserService user)
    {
        _portalReadService = portalReadService;
        _user = user;
    }

    public async Task<Result<PagedResult<PortalAssignmentDto>>> Handle(
        GetParentChildAssignmentsQuery request, CancellationToken ct)
    {
        var userId = _user.UserId;
        if (userId is null)
            return Result.Failure<PagedResult<PortalAssignmentDto>>("Not authenticated.");

        var p = request.Pagination ?? new PaginationParams();
        var result = await _portalReadService.GetParentChildAssignmentsAsync(
            request.SchoolId, userId.Value, request.StudentId, p, ct);

        return Result.Success(result);
    }
}
