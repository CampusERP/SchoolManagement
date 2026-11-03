namespace Application.Features.Subjects.Queries.GetGradeCurriculum;

public record CurriculumBookDto(Guid Id, string Title, string? Author,
    string? Publisher, string? Isbn, string? Edition, string? Notes,
    List<Application.Features.Documents.DocumentDto>? Documents = null);
