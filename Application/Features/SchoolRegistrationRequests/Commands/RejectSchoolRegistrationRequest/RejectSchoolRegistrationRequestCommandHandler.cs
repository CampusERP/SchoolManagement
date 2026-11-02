using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Domain.Entities.Tenancy;
using MediatR;

namespace Application.Features.SchoolRegistrationRequests.Commands.RejectSchoolRegistrationRequest;

public class RejectSchoolRegistrationRequestCommandHandler
    : IRequestHandler<RejectSchoolRegistrationRequestCommand, Result>
{
    private readonly ISchoolRegistrationRequestRepository _requests;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailNotifier _emailNotifier;

    public RejectSchoolRegistrationRequestCommandHandler(
        ISchoolRegistrationRequestRepository requests,
        ICurrentUserService currentUser,
        IEmailNotifier emailNotifier)
    {
        _requests = requests;
        _currentUser = currentUser;
        _emailNotifier = emailNotifier;
    }

    public async Task<Result> Handle(
        RejectSchoolRegistrationRequestCommand request, CancellationToken ct)
    {
        var entity = await _requests.GetByIdAsync(request.RequestId, ct);
        if (entity is null)
            return Result.Failure("Registration request not found.");

        if (entity.Status != SchoolRegistrationRequestStatus.Pending)
            return Result.Failure($"Only pending requests can be rejected (current status: {entity.Status}).");

        var reviewerId = _currentUser.UserId
            ?? throw new ForbiddenException("Unable to determine the reviewing user.");

        entity.Reject(reviewerId, request.Reason);

        await _emailNotifier.SendToEmailAsync(
            entity.AdminEmail,
            EmailTemplate.SchoolRegistrationRejected,
            new Dictionary<string, string?>
            {
                ["ContactName"] = $"{entity.AdminFirstName} {entity.AdminLastName}".Trim(),
                ["SchoolName"] = entity.SchoolName,
                ["Reason"] = string.IsNullOrWhiteSpace(request.Reason)
                    ? "The registration details could not be verified."
                    : request.Reason
            },
            ct);

        return Result.Success();
    }
}
