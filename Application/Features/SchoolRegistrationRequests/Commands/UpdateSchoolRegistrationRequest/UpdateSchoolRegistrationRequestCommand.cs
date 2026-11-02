using Application.Common.Behaviors;

namespace Application.Features.SchoolRegistrationRequests.Commands.UpdateSchoolRegistrationRequest;

/// <summary>
/// Edits a pending registration request. Platform super admin only.
/// The admin password is optional — when omitted, the existing one is kept.
/// </summary>
public record UpdateSchoolRegistrationRequestCommand(
    Guid RequestId,
    string SchoolName,
    string SubdomainCode,
    string? Address,
    string? Phone,
    string? SchoolEmail,
    string AdminFirstName,
    string AdminLastName,
    string AdminEmail,
    string? AdminPassword = null,
    string? ReviewNotes = null,
    Guid? SubscriptionPlanId = null)
    : ICommand, IBaseCommand;
