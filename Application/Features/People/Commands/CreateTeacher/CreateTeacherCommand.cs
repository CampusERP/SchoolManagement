using Application.Common.Behaviors;

namespace Application.Features.People.Commands.CreateTeacher;

public record CreateTeacherCommand(
    Guid ApplicationUserId,
    string EmployeeCode,
    string FirstName,
    string LastName) : ICommand<Guid>, IBaseCommand;
