using Domain.Entities.Exams;

namespace Application.Common.Interfaces.Repositories;

public interface IExamRepository
{
    Task<Exam?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Exam exam, CancellationToken ct = default);
}
