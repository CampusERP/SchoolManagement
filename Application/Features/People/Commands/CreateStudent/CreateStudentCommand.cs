using Application.Common.Behaviors;

namespace Application.Features.People.Commands.CreateStudent;

public record CreateStudentCommand(
    string StudentCode,
    string FirstName,
    string LastName,
    DateTime DateOfBirth) : ICommand<Guid>, IBaseCommand;
