namespace Application.Features.People.Queries.StudentDetails;

public record StudentDetailDto(Guid Id, string StudentCode, string FirstName,
    string LastName, DateTime DateOfBirth, bool HasLogin,
    List<StudentEnrollmentSummaryDto> Enrollments,
    List<GuardianSummaryDto> Guardians);