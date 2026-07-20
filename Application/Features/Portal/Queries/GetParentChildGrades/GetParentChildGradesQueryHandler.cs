using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Application.Features.Portal.Queries.Shared;
using MediatR;

namespace Application.Features.Portal.Queries.GetParentChildGrades;

public class GetParentChildGradesQueryHandler
    : IRequestHandler<GetParentChildGradesQuery, Result<PagedResult<PortalExamResultDto>>>
{
    private readonly IPortalReadService _portalReadService;
    private readonly ICurrentUserService _user;

    public GetParentChildGradesQueryHandler(IPortalReadService portalReadService, ICurrentUserService user)
    {
        _portalReadService = portalReadService;
        _user = user;
    }

    public async Task<Result<PagedResult<PortalExamResultDto>>> Handle(
        GetParentChildGradesQuery request, CancellationToken ct)
    {
        var userId = _user.UserId;
        if (userId is null)
            return Result.Failure<PagedResult<PortalExamResultDto>>("Not authenticated.");

        var p = request.Pagination ?? new PaginationParams();
        var result = await _portalReadService.GetParentChildGradesAsync(
            request.SchoolId, userId.Value, request.StudentId, request.TermId, p, ct);

        return Result.Success(result);
    }
}
