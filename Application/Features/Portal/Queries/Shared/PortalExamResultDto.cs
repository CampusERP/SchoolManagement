namespace Application.Features.Portal.Queries.Shared;

public record PortalExamResultDto(
    string  SubjectName,
    string  ExamName,
    decimal Score,
    decimal MaxScore,
    decimal Percentage,
    string  Grade,
    string? Remarks);
