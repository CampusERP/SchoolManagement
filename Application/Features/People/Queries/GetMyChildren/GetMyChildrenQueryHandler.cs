using Application.Common.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Queries.GetMyChildren;

public class GetMyChildrenQueryHandler
    : IRequestHandler<GetMyChildrenQuery, Result<List<ChildSummaryDto>>>
{
    private readonly IParentReadService _parentReadService;
    private readonly ICurrentUserService _user;

    public GetMyChildrenQueryHandler(IParentReadService parentReadService, ICurrentUserService user)
    {
        _parentReadService = parentReadService;
        _user = user;
    }

    public async Task<Result<List<ChildSummaryDto>>> Handle(
        GetMyChildrenQuery request, CancellationToken ct)
    {
        var userId = _user.UserId;
        if (userId is null) return Result.Failure<List<ChildSummaryDto>>("Not authenticated.");

        var children = await _parentReadService.GetMyChildrenAsync(request.SchoolId, userId.Value, ct);

        return Result.Success(children);
    }
}
