using Application.Common.Behaviors;

namespace Application.Features.SchoolRegistrationRequests.Commands.RejectSchoolRegistrationRequest;

/// <summary>
/// Rejects a pending registration request. Platform super admin only.
/// </summary>
public record RejectSchoolRegistrationRequestCommand(Guid RequestId, string? Reason = null)
    : ICommand, IBaseCommand;
