using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Identity.SwitchSchool;

public class GetUserMembershipsQueryHandler
    : IRequestHandler<GetUserMembershipsQuery, Result<List<UserMembershipDto>>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUserSchoolMembershipRepository _memberships;
    private readonly ISchoolRepository _schools;
    private readonly ITenantContext _tenant;

    public GetUserMembershipsQueryHandler(
        ICurrentUserService currentUser,
        IUserSchoolMembershipRepository memberships,
        ISchoolRepository schools,
        ITenantContext tenant)
    {
        _currentUser = currentUser;
        _memberships = memberships;
        _schools = schools;
        _tenant = tenant;
    }

    public async Task<Result<List<UserMembershipDto>>> Handle(
        GetUserMembershipsQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Result.Failure<List<UserMembershipDto>>("Not authenticated.");

        var activeMemberships = await _memberships.GetActiveMembershipsAsync(userId.Value, ct);

        var result = new List<UserMembershipDto>();
        foreach (var m in activeMemberships)
        {
            var school = await _schools.GetByIdAsync(m.SchoolId, ct);
            if (school is null) continue;
            result.Add(new UserMembershipDto(
                m.SchoolId, school.Name, school.SubdomainCode,
                m.Role, m.IsActive, m.SchoolId == _tenant.SchoolId));
        }

        return Result.Success(result.OrderBy(r => r.SchoolName).ToList());
    }
}
