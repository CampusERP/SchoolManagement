using Application.Common.Constants;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Identity.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly IUserSchoolMembershipRepository _memberships;
    private readonly ISchoolRepository _schools;
    private readonly IJwtTokenService _jwtService;
    private readonly IPermissionProvider _permissions;

    public LoginCommandHandler(
            IIdentityService identityService,
            IUserSchoolMembershipRepository memberships,
            ISchoolRepository schools,
            IJwtTokenService jwtService,
            IPermissionProvider permissions)
    {
        _identityService = identityService;
        _memberships = memberships;
        _schools = schools;
        _jwtService = jwtService;
        _permissions = permissions;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var attempt = await _identityService.ValidateCredentialsAsync(request.Email, request.Password, ct);

        switch (attempt.Status)
        {
            case SignInStatus.LockedOut:
                return Result.Failure<LoginResponse>("Account is locked. Try again later.");
            case SignInStatus.InvalidCredentials:
                return Result.Failure<LoginResponse>("Invalid credentials.");
        }

        var user = attempt.User!;

        Guid? schoolId = null;
        string role = "Unknown";
        List<string> permissions;

        if (user.IsPlatformAdmin)
        {
            role = "SuperAdmin";
            permissions = _permissions.GetPlatformAdminPermissions();
        }
        else
        {
            var activeMemberships = await _memberships.GetActiveMembershipsAsync(user.Id, ct);

            // Suspended schools keep their memberships, but no user may sign in
            // to them. Resolve which schools are usable in a single query so a
            // user with memberships at several schools can still sign in to an
            // active one.
            var schoolIds = activeMemberships.Select(m => m.SchoolId).Distinct().ToList();
            var activeSchoolIds = (await _schools.GetActiveSchoolIdsAsync(schoolIds, ct)).ToHashSet();

            if (request.SchoolId.HasValue && !activeSchoolIds.Contains(request.SchoolId.Value))
            {
                var requestedMembership = activeMemberships
                    .FirstOrDefault(m => m.SchoolId == request.SchoolId.Value);

                return Result.Failure<LoginResponse>(requestedMembership is null
                    ? "No active school membership found."
                    : SchoolAccessMessages.Suspended);
            }

            var membership = request.SchoolId.HasValue
                ? activeMemberships.FirstOrDefault(m => m.SchoolId == request.SchoolId.Value)
                : activeMemberships.FirstOrDefault(m => activeSchoolIds.Contains(m.SchoolId));

            if (membership is null)
                return Result.Failure<LoginResponse>(activeMemberships.Count > 0
                    ? SchoolAccessMessages.Suspended
                    : "No active school membership found.");

            schoolId = membership.SchoolId;

            var roles = await _identityService.GetRolesAsync(user.Id, ct);
            role = roles.FirstOrDefault() ?? "Unknown";

            permissions = _permissions.GetPermissionsForRole(role);
        }

        var tokens = await _jwtService.IssueTokensAsync(
            user.Id, user.Email, schoolId, permissions, user.IsPlatformAdmin, ct);

        return Result.Success(new LoginResponse(
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.AccessTokenExpiry,
            user.Id,
            user.Email,
            schoolId,
            role));
    }
}