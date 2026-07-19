namespace Application.Features.People.Queries.StudentDetails;

public record StudentDetailDto(Guid Id, string StudentCode, string FirstName,
    string LastName, DateTime DateOfBirth, string? NationalId, bool HasLogin,
    List<StudentEnrollmentSummaryDto> Enrollments,
    List<GuardianSummaryDto> Guardians);