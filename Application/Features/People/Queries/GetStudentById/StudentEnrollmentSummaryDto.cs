namespace Application.Features.People.Queries.StudentDetails;

public record StudentEnrollmentSummaryDto(Guid EnrollmentId, string AcademicYear,
    string ClassRoom, string GradeLevel, string Status);