namespace Application.Features.Portal.Queries.GetTeacherSchedule;

public record RosterStudentDto(
    Guid   EnrollmentId,
    Guid   StudentId,
    string StudentCode,
    string FirstName,
    string LastName,
    string EnrollmentStatus);
