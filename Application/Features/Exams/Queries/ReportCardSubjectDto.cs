namespace Application.Features.Exams.Queries;

public record ReportCardSubjectDto(string SubjectName, decimal Score,
    decimal MaxScore, string Grade);
