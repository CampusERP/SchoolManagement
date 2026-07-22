namespace Application.Features.People.Queries.GetStudents;

public record StudentListDto(Guid Id, string StudentCode, string FirstName,
    string LastName, string? Email, DateTime DateOfBirth, string? CurrentClass, string? EnrollmentStatus);