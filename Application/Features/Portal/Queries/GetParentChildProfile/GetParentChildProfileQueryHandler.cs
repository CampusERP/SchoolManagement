using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Portal.Queries.GetParentChildProfile;

public class GetParentChildProfileQueryHandler
    : IRequestHandler<GetParentChildProfileQuery, Result<ParentChildProfileDto>>
{
    private readonly IPortalReadService _portalReadService;
    private readonly ICurrentUserService _user;

    public GetParentChildProfileQueryHandler(IPortalReadService portalReadService, ICurrentUserService user)
    {
        _portalReadService = portalReadService;
        _user = user;
    }

    public async Task<Result<ParentChildProfileDto>> Handle(
        GetParentChildProfileQuery request, CancellationToken ct)
    {
        var userId = _user.UserId;
        if (userId is null)
            return Result.Failure<ParentChildProfileDto>("Not authenticated.");

        var profile = await _portalReadService.GetParentChildProfileAsync(
            request.SchoolId, userId.Value, request.StudentId, ct);

        if (profile is null)
            return Result.Failure<ParentChildProfileDto>(
                "Child not found or access denied.");

        return Result.Success(profile);
    }
}
