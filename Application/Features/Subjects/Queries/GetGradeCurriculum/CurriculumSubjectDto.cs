namespace Application.Features.Subjects.Queries.GetGradeCurriculum;

public record CurriculumSubjectDto(Guid CurriculumSubjectId, Guid SubjectId,
    string Code, string Name, string? Description, bool IsSchoolSpecific, List<CurriculumBookDto> Books);
