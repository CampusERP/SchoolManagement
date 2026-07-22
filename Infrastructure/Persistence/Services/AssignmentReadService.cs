using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Application.Features.Assignments.Queries.GetClassAssignments;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Services;

public class AssignmentReadService : IAssignmentReadService
{
    private readonly ApplicationDbContext _db;

    public AssignmentReadService(ApplicationDbContext db) => _db = db;

    public async Task<PagedResult<AssignmentSummaryDto>> GetClassAssignmentsAsync(
        Guid teachingAssignmentId, PaginationParams p, CancellationToken ct = default)
    {
        var query = _db.Assignments.AsNoTracking()
            .Where(a => a.TeachingAssignmentId == teachingAssignmentId);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.DueDate)
            .Skip(p.Skip)
            .Take(p.PageSize)
            .Select(a => new AssignmentSummaryDto(
                a.Id,
                a.Title,
                a.DueDate,
                a.MaxScore,
                a.Submissions.Count,
                a.Submissions.Count(s => s.Status == SubmissionStatus.Graded),
                a.Submissions.Count(s => s.Status != SubmissionStatus.Graded)))
            .ToListAsync(ct);

        return new PagedResult<AssignmentSummaryDto>(items, total, p.Page, p.PageSize);
    }

    public async Task<PagedResult<StudentAssignmentDto>> GetStudentAssignmentsAsync(
        Guid enrollmentId, PaginationParams p, CancellationToken ct = default)
    {
        var enrollment = await _db.StudentEnrollments.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == enrollmentId, ct);

        if (enrollment is null)
            return PagedResult<StudentAssignmentDto>.Empty(p.Page, p.PageSize);

        var teachingAssignmentIds = await _db.TeachingAssignments.AsNoTracking()
            .Where(ta => ta.ClassRoomId == enrollment.ClassRoomId)
            .Select(ta => ta.Id)
            .ToListAsync(ct);

        var baseQuery = _db.Assignments.AsNoTracking()
            .Where(a => teachingAssignmentIds.Contains(a.TeachingAssignmentId));

        var total = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .OrderByDescending(a => a.DueDate)
            .Skip(p.Skip)
            .Take(p.PageSize)
            .Select(a => new StudentAssignmentDto(
                a.Id,
                a.Title,
                a.Instructions,
                a.DueDate,
                a.MaxScore,
                _db.AssignmentSubmissions
                    .Where(s => s.AssignmentId == a.Id && s.StudentEnrollmentId == enrollmentId)
                    .Select(s => s.Status)
                    .FirstOrDefault(),
                _db.AssignmentSubmissions
                    .Where(s => s.AssignmentId == a.Id && s.StudentEnrollmentId == enrollmentId)
                    .Select(s => s.Grade)
                    .FirstOrDefault(),
                _db.AssignmentSubmissions
                    .Where(s => s.AssignmentId == a.Id && s.StudentEnrollmentId == enrollmentId)
                    .Select(s => s.TeacherFeedback)
                    .FirstOrDefault(),
                _db.AssignmentSubmissions
                    .Where(s => s.AssignmentId == a.Id && s.StudentEnrollmentId == enrollmentId)
                    .Select(s => s.SubmittedAtUtc)
                    .FirstOrDefault(),
                new List<string>()))
            .ToListAsync(ct);

        return new PagedResult<StudentAssignmentDto>(items, total, p.Page, p.PageSize);
    }
}
