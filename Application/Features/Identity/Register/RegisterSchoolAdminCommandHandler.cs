using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Messages;
using Application.Common.Models;
using Domain.Entities.Tenancy;
using MediatR;

namespace Application.Features.Identity.Register;

public class RegisterSchoolAdminCommandHandler
    : IRequestHandler<RegisterSchoolAdminCommand, Result<Guid>>
{
    private readonly IIdentityService _identity;
    private readonly ISchoolRepository _schools;
    private readonly IUserSchoolMembershipRepository _memberships;
    private readonly IOutboxService _outbox;

    public RegisterSchoolAdminCommandHandler(
        IIdentityService identity,
        ISchoolRepository schools,
        IUserSchoolMembershipRepository memberships,
        IOutboxService outbox)
    {
        _identity = identity;
        _schools = schools;
        _memberships = memberships;
        _outbox = outbox;
    }

    public async Task<Result<Guid>> Handle(
        RegisterSchoolAdminCommand request, CancellationToken ct)
    {
        var school = await _schools.GetByIdAsync(request.SchoolId, ct);
        if (school is null)
            throw new NotFoundException(nameof(School), request.SchoolId);

        var createResult = await _identity.CreateUserAsync(
            request.Email, request.Password, ct);
        if (!createResult.IsSuccess)
            return Result.Failure<Guid>(createResult.Error!);

        var userId = createResult.Value;

        try
        {
            var roleResult = await _identity.AddToRoleAsync(userId, "SchoolAdmin", ct);
            if (!roleResult.IsSuccess)
                return Result.Failure<Guid>(roleResult.Error!);

            var membership = UserSchoolMembership.Create(userId, request.SchoolId, "SchoolAdmin");
            await _memberships.AddAsync(membership, ct);

            _outbox.Publish(new CreateSchoolAdminProfileMessage(
                userId, request.SchoolId, request.FirstName, request.LastName));

            return Result.Success(userId);
        }
        catch
        {
            await _identity.DeleteUserAsync(userId, ct);
            throw;
        }
    }
}