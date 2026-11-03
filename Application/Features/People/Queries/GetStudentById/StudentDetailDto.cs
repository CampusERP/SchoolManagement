namespace Application.Features.People.Queries.StudentDetails;

public record StudentDetailDto(Guid Id, string StudentCode, string FirstName,
    string LastName, DateTime DateOfBirth, string? NationalId, bool HasLogin,
    List<StudentEnrollmentSummaryDto> Enrollments,
    List<GuardianSummaryDto> Guardians,
    string? SecondName = null,
    string? ThirdName = null,
    string? NationalityCountry = null,
    string? ResidenceCountry = null,
    List<Application.Features.Documents.DocumentDto>? Documents = null,
    string? Email = null,
    Guid? CampusId = null);
