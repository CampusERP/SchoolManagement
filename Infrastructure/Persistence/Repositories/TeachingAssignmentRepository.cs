using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.Enrollment;

namespace Infrastructure.Persistence.Repositories;

public class TeachingAssignmentRepository : ITeachingAssignmentRepository
{
    private readonly ApplicationDbContext _context;

    public TeachingAssignmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeachingAssignment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.TeachingAssignments.Include(a => a.Schedules).FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<bool> ExistsAsync(Guid schoolId, Guid teacherId, Guid classRoomId, Guid subjectId, Guid termId, CancellationToken ct = default) =>
        await _context.TeachingAssignments.AnyAsync(a =>
            a.SchoolId == schoolId && a.TeacherId == teacherId &&
            a.ClassRoomId == classRoomId && a.SubjectId == subjectId &&
            a.TermId == termId, ct);

    public async Task<IReadOnlyList<TeachingAssignment>> GetByTeacherAndTermAsync(Guid schoolId, Guid teacherId, Guid termId, CancellationToken ct = default) =>
        await _context.TeachingAssignments
            .Include(a => a.Schedules)
            .Where(a => a.SchoolId == schoolId && a.TeacherId == teacherId && a.TermId == termId)
            .ToListAsync(ct);

    public async Task AddAsync(TeachingAssignment assignment, CancellationToken ct = default) =>
        await _context.TeachingAssignments.AddAsync(assignment, ct);

    public Task UpdateAsync(TeachingAssignment assignment, CancellationToken ct = default)
    {
        if (_context.Entry(assignment).State == EntityState.Detached)
            _context.TeachingAssignments.Update(assignment);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(TeachingAssignment assignment, CancellationToken ct = default)
    {
        _context.TeachingAssignments.Remove(assignment);
        await Task.CompletedTask;
    }
}
