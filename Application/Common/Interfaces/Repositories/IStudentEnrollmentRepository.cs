using Domain.Entities.Enrollment;

namespace Application.Common.Interfaces.Repositories;

public interface IStudentEnrollmentRepository
{
    Task<StudentEnrollment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<StudentEnrollment?> GetActiveAsync(Guid schoolId, Guid studentId, Guid academicYearId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid schoolId, Guid studentId, Guid academicYearId, CancellationToken ct = default);
    Task AddAsync(StudentEnrollment enrollment, CancellationToken ct = default);
}
