namespace Application.Features.Portal.Queries.Shared;

public record PortalReportCardDto(
    Guid    ReportCardId,
    string  TermName,
    decimal OverallPercentage,
    string  OverallGrade,
    bool    IsLocked,
    DateTime GeneratedAtUtc,
    List<PortalReportCardSubjectDto> Subjects);

public record PortalReportCardSubjectDto(
    string  SubjectName,
    decimal Score,
    decimal MaxScore,
    string  Grade);
