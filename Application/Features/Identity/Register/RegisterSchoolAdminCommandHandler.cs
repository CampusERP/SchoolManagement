using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Domain.Entities.People;
using Domain.Entities.Tenancy;
using MediatR;


namespace Application.Features.Identity.Register;

public class RegisterSchoolAdminCommandHandler : IRequestHandler<RegisterSchoolAdminCommand, Result<Guid>>
{
    private readonly IIdentityService _identityService;
    private readonly ISchoolRepository _schools;
    private readonly IUserSchoolMembershipRepository _userSchoolMembership;
    private readonly ISchoolAdminProfileRepository _schoolAdminProfile;

    public RegisterSchoolAdminCommandHandler(IIdentityService identityService,
        ISchoolRepository schoolRepository,
        IUserSchoolMembershipRepository userSchoolMembershipRepository,
        ISchoolAdminProfileRepository schoolAdminProfileRepository)
    {
        _identityService = identityService;
        _schools = schoolRepository;
        _userSchoolMembership = userSchoolMembershipRepository;
        _schoolAdminProfile = schoolAdminProfileRepository;
    }

    public async Task<Result<Guid>> Handle(RegisterSchoolAdminCommand request, CancellationToken ct)
    {
        var school = await _schools.GetByIdAsync(request.SchoolId, ct);
        if (school is null)
            throw new NotFoundException(nameof(School), request.SchoolId);

        var createResult = await _identityService.CreateUserAsync(request.Email, request.Password, ct);
        if (!createResult.IsSuccess)
            return Result.Failure<Guid>(createResult.Error!);

        var userId = createResult.Value;

        try
        {
            var roleResult = await _identityService.AddToRoleAsync(userId, "SchoolAdmin", ct);
            if (!roleResult.IsSuccess)
            {
                await _identityService.DeleteUserAsync(userId, ct);
                return Result.Failure<Guid>(roleResult.Error!);
            }

            var membership = UserSchoolMembership.Create(userId, request.SchoolId);
            await _userSchoolMembership.AddAsync(membership, ct);

            var profile = SchoolAdminProfile.Create(
                request.SchoolId, userId, request.FirstName, request.LastName);
            await _schoolAdminProfile.AddAsync(profile, ct);

            return Result.Success(userId);
        }
        catch
        {
            await _identityService.DeleteUserAsync(userId, ct);
            throw;
        }
    }
}
