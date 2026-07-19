namespace Application.Features.Academics.Queries.GetSubjects;

public record SubjectDto(Guid Id, string Code, string Name, string? Description);