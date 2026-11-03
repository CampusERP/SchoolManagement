using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.People.Commands.CreateTeacher;

public record CreateTeacherCommand(
    Guid SchoolId,
    string EmployeeCode,
    string FirstName,
    string LastName,
    string Email,
    string Password,
    List<Guid>? CampusIds = null) : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;
