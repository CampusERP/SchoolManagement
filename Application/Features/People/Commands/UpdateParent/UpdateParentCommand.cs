using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.People.Commands.UpdateParent;

public record UpdateParentCommand(
    Guid SchoolId,
    Guid ParentId,
    string FirstName,
    string LastName) : ICommand, IBaseCommand, ITenantScopedRequest;
