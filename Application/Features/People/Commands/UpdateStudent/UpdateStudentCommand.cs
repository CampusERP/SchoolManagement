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
    string? NationalId = null,
    string? SecondName = null,
    string? ThirdName = null,
    string? NationalityCountry = null,
    string? ResidenceCountry = null,
    Guid? CampusId = null) : ICommand, IBaseCommand, ITenantScopedRequest;
