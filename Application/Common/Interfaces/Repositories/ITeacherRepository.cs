using Domain.Entities.People;

namespace Application.Common.Interfaces.Repositories;

public interface ITeacherRepository
{
    Task<Teacher?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid schoolId, string employeeCode, CancellationToken ct = default);
    Task AddAsync(Teacher teacher, CancellationToken ct = default);
}
