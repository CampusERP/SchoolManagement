namespace Application.Features.Exams.Queries;

public record ClassExamResultDto(Guid EnrollmentId, string StudentCode,
    string FirstName, string LastName, decimal Score, decimal Percentage, string? Remarks);
