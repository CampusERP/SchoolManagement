using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Identity.SwitchSchool;

public class SwitchSchoolCommandHandler
    : IRequestHandler<SwitchSchoolCommand, Result<SwitchSchoolResponse>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUserSchoolMembershipRepository _memberships;
    private readonly ISchoolRepository _schools;
    private readonly IJwtTokenService _jwtService;
    private readonly IPermissionProvider _permissionProvider;

    public SwitchSchoolCommandHandler(
        ICurrentUserService currentUser,
        IUserSchoolMembershipRepository memberships,
        ISchoolRepository schools,
        IJwtTokenService jwtService,
        IPermissionProvider permissionProvider)
    {
        _currentUser = currentUser;
        _memberships = memberships;
        _schools = schools;
        _jwtService = jwtService;
        _permissionProvider = permissionProvider;
    }

    public async Task<Result<SwitchSchoolResponse>> Handle(
        SwitchSchoolCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result.Failure<SwitchSchoolResponse>("No authenticated user.");

        var membership = await _memberships.GetAsync(userId.Value, request.TargetSchoolId, ct);
        if (membership is null || !membership.IsActive)
            return Result.Failure<SwitchSchoolResponse>(
                "You do not have an active membership at this school.");

        var school = await _schools.GetByIdAsync(request.TargetSchoolId, ct);
        if (school is null)
            return Result.Failure<SwitchSchoolResponse>("School not found.");

        var permissions = _permissionProvider.GetPermissionsForRole(membership.Role);

        var tokens = await _jwtService.IssueTokensAsync(
            userId.Value, _currentUser.Email!, request.TargetSchoolId,
            permissions, isPlatformAdmin: false, ct);

        return Result.Success(new SwitchSchoolResponse(
            tokens.AccessToken, tokens.RefreshToken, tokens.AccessTokenExpiry,
            request.TargetSchoolId, membership.Role, school.Name));
    }
}
