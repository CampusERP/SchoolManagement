using Application.Common.Behaviors;

namespace Application.Features.People.Commands.CreateParent;

public record CreateParentCommand(
    Guid ApplicationUserId,
    string FirstName,
    string LastName) : ICommand<Guid>, IBaseCommand;
