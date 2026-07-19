using Domain.Entities.Academics;

namespace Application.Common.Interfaces.Repositories;

public interface IAcademicYearRepository
{
    Task<AcademicYear?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AcademicYear?> GetCurrentAsync(Guid schoolId, CancellationToken ct = default);
    Task<bool> HasCurrentAsync(Guid schoolId, CancellationToken ct = default);
    Task AddAsync(AcademicYear year, CancellationToken ct = default);
    Task<Guid> AddTermAsync(AcademicYear year, CancellationToken ct = default);
}
