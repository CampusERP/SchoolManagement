namespace Application.Features.Academics.Queries.GetTerms;

public record TermDto(
    Guid Id,
    string Name,
    int Sequence,
    DateTime StartDate,
    DateTime EndDate);
