namespace Application.Features.Academics.Queries.GetAcademicYears;

public record TermDto(Guid Id, string Name, int Sequence, DateTime StartDate, DateTime EndDate);

public record AcademicYearDto(Guid Id, string Name, DateTime StartDate,
    DateTime EndDate, bool IsCurrent, string Status, List<TermDto> Terms);
