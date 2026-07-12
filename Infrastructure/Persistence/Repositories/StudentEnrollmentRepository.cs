using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.Enrollment;
using Domain.Enums;

namespace Infrastructure.Persistence.Repositories;

public class StudentEnrollmentRepository : IStudentEnrollmentRepository
{
    private readonly ApplicationDbContext _context;

    public StudentEnrollmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentEnrollment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.StudentEnrollments.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<StudentEnrollment?> GetActiveAsync(Guid schoolId, Guid studentId, Guid academicYearId, CancellationToken ct = default) =>
        await _context.StudentEnrollments.FirstOrDefaultAsync(e =>
            e.SchoolId == schoolId && e.StudentId == studentId &&
            e.AcademicYearId == academicYearId && e.Status == EnrollmentStatus.Active, ct);

    public async Task<bool> ExistsAsync(Guid schoolId, Guid studentId, Guid academicYearId, CancellationToken ct = default) =>
        await _context.StudentEnrollments.AnyAsync(e =>
            e.SchoolId == schoolId && e.StudentId == studentId && e.AcademicYearId == academicYearId, ct);

    public async Task AddAsync(StudentEnrollment enrollment, CancellationToken ct = default) =>
        await _context.StudentEnrollments.AddAsync(enrollment, ct);
}
