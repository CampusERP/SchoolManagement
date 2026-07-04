using Domain.Entities.People;

namespace Application.Common.Interfaces.Repositories;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid schoolId, string studentCode, CancellationToken ct = default);
    Task AddAsync(Student student, CancellationToken ct = default);
}
