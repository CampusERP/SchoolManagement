using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.People.Commands.DeleteStudent;

public record DeleteStudentCommand(Guid SchoolId, Guid StudentId)
    : ICommand, ITenantScopedRequest;
