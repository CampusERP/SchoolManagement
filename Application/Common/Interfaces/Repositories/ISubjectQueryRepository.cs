using Application.Features.Academics.Queries.GetSubjects;

namespace Application.Common.Interfaces.Repositories;

public interface ISubjectQueryRepository
{
    Task<List<SubjectDto>> GetSubjectsAsync(
        Guid? gradeLevelId,
        CancellationToken cancellationToken = default);
}