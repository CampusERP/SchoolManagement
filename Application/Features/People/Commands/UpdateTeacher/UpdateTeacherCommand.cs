using Application.Common.Behaviors;
using Application.Common.Interfaces;
using Domain.Enums;

namespace Application.Features.People.Commands.UpdateTeacher;

public record UpdateTeacherCommand(
    Guid SchoolId,
    Guid TeacherId,
    string EmployeeCode,
    string FirstName,
    string LastName,
    EmploymentStatus EmploymentStatus) : ICommand, IBaseCommand, ITenantScopedRequest;
