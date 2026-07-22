using Domain.Entities.Enrollment;

namespace Application.Common.Interfaces.Repositories;

public interface ITeachingAssignmentRepository
{
    Task<TeachingAssignment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid schoolId, Guid teacherId, Guid classRoomId, Guid subjectId, Guid termId, CancellationToken ct = default);

    Task<IReadOnlyList<TeachingAssignment>> GetByTeacherAndTermAsync(Guid schoolId, Guid teacherId, Guid termId, CancellationToken ct = default);

    Task AddAsync(TeachingAssignment assignment, CancellationToken ct = default);
    Task UpdateAsync(TeachingAssignment assignment, CancellationToken ct = default);
    Task DeleteAsync(TeachingAssignment assignment, CancellationToken ct = default);
}
