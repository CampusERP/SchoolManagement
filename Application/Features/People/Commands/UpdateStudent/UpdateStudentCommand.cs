using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.People.Commands.UpdateStudent;

public record UpdateStudentCommand(
    Guid SchoolId,
    Guid StudentId,
    string StudentCode,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string? NationalId = null) : ICommand, IBaseCommand, ITenantScopedRequest;
