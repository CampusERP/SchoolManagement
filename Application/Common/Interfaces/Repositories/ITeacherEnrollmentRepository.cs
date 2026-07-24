using Domain.Entities.Enrollment;

namespace Application.Common.Interfaces.Repositories;

public interface ITeacherEnrollmentRepository
{
    Task<TeacherEnrollment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid schoolId, Guid teacherId, Guid termId, CancellationToken ct = default);
    Task AddAsync(TeacherEnrollment enrollment, CancellationToken ct = default);
}
