using Application.Common.Behaviors;

namespace Application.Features.SchoolRegistrationRequests.Commands.CreateSchoolRegistrationRequest;

/// <summary>
/// Submitted publicly from the "Register your school" form on the landing page.
/// Creates a pending request for a platform super admin to review.
/// </summary>
public record CreateSchoolRegistrationRequestCommand(
    string SchoolName,
    string SubdomainCode,
    string? Address,
    string? Phone,
    string? SchoolEmail,
    string AdminFirstName,
    string AdminLastName,
    string AdminEmail,
    string AdminPassword,
    Guid? SubscriptionPlanId = null)
    : ICommand<Guid>, IBaseCommand;
