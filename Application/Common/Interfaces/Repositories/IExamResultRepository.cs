using Domain.Entities.Exams;

namespace Application.Common.Interfaces.Repositories;

public interface IExamResultRepository
{
    Task<List<ExamResult>> GetResultsForReportCardAsync(Guid enrollmentId, Guid termId, CancellationToken ct = default);
}
