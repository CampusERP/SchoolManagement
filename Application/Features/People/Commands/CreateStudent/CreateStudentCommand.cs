using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.People.Commands.CreateStudent;

public record CreateStudentCommand(
    Guid SchoolId,
    string StudentCode,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string? Email = null,
    string? Password = null) : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;