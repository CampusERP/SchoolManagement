using Application.Common.Behaviors;

namespace Application.Features.SchoolRegistrationRequests.Commands.DeleteSchoolRegistrationRequest;

/// <summary>
/// Deletes a school registration request. Platform super admin only.
/// </summary>
public record DeleteSchoolRegistrationRequestCommand(Guid RequestId)
    : ICommand, IBaseCommand;
