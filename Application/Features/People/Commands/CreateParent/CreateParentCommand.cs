using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.People.Commands.CreateParent;

public record CreateParentCommand(
    Guid SchoolId,
    string FirstName,
    string LastName,
    string Email,
    string Password) : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;
