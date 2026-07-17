using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.People.Commands.CreateTeacher;

public record CreateTeacherCommand(
    Guid SchoolId,
    string EmployeeCode,
    string FirstName,
    string LastName,
    string Email,
    string Password) : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;