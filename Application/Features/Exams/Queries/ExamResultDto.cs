namespace Application.Features.Exams.Queries;

public record ExamResultDto(string SubjectName, decimal Score,
    decimal MaxScore, decimal Percentage, string Grade, string? Remarks);
