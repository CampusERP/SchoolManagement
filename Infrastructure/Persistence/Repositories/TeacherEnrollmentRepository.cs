using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.Enrollment;

namespace Infrastructure.Persistence.Repositories;

public class TeacherEnrollmentRepository : ITeacherEnrollmentRepository
{
    private readonly ApplicationDbContext _context;

    public TeacherEnrollmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeacherEnrollment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.TeacherEnrollments.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<bool> ExistsAsync(Guid schoolId, Guid teacherId, Guid termId, CancellationToken ct = default) =>
        await _context.TeacherEnrollments.AnyAsync(e =>
            e.SchoolId == schoolId && e.TeacherId == teacherId && e.TermId == termId, ct);

    public async Task AddAsync(TeacherEnrollment enrollment, CancellationToken ct = default) =>
        await _context.TeacherEnrollments.AddAsync(enrollment, ct);
}
