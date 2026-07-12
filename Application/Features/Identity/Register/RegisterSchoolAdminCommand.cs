using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Identity.Register;

public record RegisterSchoolAdminCommand(
    Guid SchoolId,
    string FirstName,
    string LastName,
    string Email,
    string Password
    ) : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;