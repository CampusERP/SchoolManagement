namespace Application.Features.Exams.Queries;

public record ReportCardDto(Guid Id, string StudentName, string TermName,
    decimal OverallPercentage, string OverallGrade, bool IsLocked,
    DateTime GeneratedAtUtc, List<ReportCardSubjectDto> Subjects);
