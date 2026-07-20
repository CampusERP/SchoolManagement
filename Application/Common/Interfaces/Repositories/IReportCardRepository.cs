using Domain.Entities.Exams;

namespace Application.Common.Interfaces.Repositories;

public interface IReportCardRepository
{
    Task<ReportCard?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid studentEnrollmentId, Guid termId, CancellationToken ct = default);
    Task AddAsync(ReportCard card, CancellationToken ct = default);
}
