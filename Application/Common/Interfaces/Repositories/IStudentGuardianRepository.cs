using Domain.Entities.People;

namespace Application.Common.Interfaces.Repositories;

public interface IStudentGuardianRepository
{
    Task AddAsync(StudentGuardian guardian, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid studentId, Guid parentId, CancellationToken ct = default);
}
