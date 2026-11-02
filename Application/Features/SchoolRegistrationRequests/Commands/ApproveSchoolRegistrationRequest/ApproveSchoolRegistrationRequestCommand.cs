using Application.Common.Behaviors;

namespace Application.Features.SchoolRegistrationRequests.Commands.ApproveSchoolRegistrationRequest;

/// <summary>
/// Approves a pending registration request: creates the school and its first
/// school-admin account. Platform super admin only.
/// Returns the id of the newly created school.
/// </summary>
public record ApproveSchoolRegistrationRequestCommand(Guid RequestId, string? Notes = null)
    : ICommand<Guid>, IBaseCommand;
