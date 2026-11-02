using Application.Features.Subjects.Queries.GetGradeCurriculum;

namespace Application.Common.Interfaces.Services;

public interface ISubjectReadService
{
    Task<List<CurriculumSubjectDto>> GetGradeCurriculumAsync(Guid schoolId, Guid gradeLevelId, CancellationToken ct = default);
}
