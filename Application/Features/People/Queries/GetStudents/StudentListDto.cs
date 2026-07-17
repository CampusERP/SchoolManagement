namespace Application.Features.People.Queries.GetStudents;

public record StudentListDto(Guid Id, string StudentCode, string FirstName,
    string LastName, DateTime DateOfBirth, string? CurrentClass, string? EnrollmentStatus);