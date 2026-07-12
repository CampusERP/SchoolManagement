using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Identity.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly IUserSchoolMembershipRepository _memberships;
    private readonly IJwtTokenService _jwtService;

    public LoginCommandHandler(
            IIdentityService identityService,
            IUserSchoolMembershipRepository memberships,
            IJwtTokenService jwtService)
    {
        _identityService = identityService;
        _memberships = memberships;
        _jwtService = jwtService;
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
            permissions = new() { "school.create", "school.manage", "subscription.manage" };
        }
        else
        {
            var activeMemberships = await _memberships.GetActiveMembershipsAsync(user.Id, ct);

            var membership = request.SchoolId.HasValue
                ? activeMemberships.FirstOrDefault(m => m.SchoolId == request.SchoolId.Value)
                : activeMemberships.FirstOrDefault();

            if (membership is null)
                return Result.Failure<LoginResponse>("No active school membership found.");

            schoolId = membership.SchoolId;

            var roles = await _identityService.GetRolesAsync(user.Id, ct);
            role = roles.FirstOrDefault() ?? "Unknown";

            permissions = GetDefaultPermissionsForRole(role);
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

    private static List<string> GetDefaultPermissionsForRole(string role) => role switch
    {
        "SchoolAdmin" => new()
        {
            "school.read", "teacher.create", "teacher.read",
            "student.create", "student.read", "enrollment.create",
            "classroom.create", "academicyear.create", "schedule.create",
            "attendance.read", "report.read"
        },
        "Teacher" => new()
        {
            "attendance.record", "grade.enter", "assignment.create",
            "classroom.read", "schedule.read"
        },
        "Student" => new()
        {
            "grade.read.own", "attendance.read.own",
            "schedule.read", "assignment.read"
        },
        "Parent" => new()
        {
            "grade.read.child", "attendance.read.child", "notification.read"
        },
        _ => new()
    };
}