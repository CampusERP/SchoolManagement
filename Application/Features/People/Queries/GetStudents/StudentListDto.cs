namespace Application.Features.People.Queries.GetStudents;

public record StudentListDto(Guid Id, string StudentCode, string FirstName,
    string LastName, string? Email, DateTime DateOfBirth, string? CurrentClass, string? EnrollmentStatus,
    string? SecondName = null, string? ThirdName = null, Guid? CampusId = null, bool HasLogin = false,
    string? NationalId = null, string? NationalityCountry = null, string? ResidenceCountry = null);
