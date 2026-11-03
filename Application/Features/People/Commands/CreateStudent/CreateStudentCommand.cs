using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.People.Commands.CreateStudent;

public record CreateStudentCommand(
    Guid SchoolId,
    string? StudentCode,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string? NationalId = null,
    string? Email = null,
    string? Password = null,
    string? SecondName = null,
    string? ThirdName = null,
    string? NationalityCountry = null,
    string? ResidenceCountry = null,
    Guid? CampusId = null) : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;
