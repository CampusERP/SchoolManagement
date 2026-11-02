using Application.Common.Interfaces.Repositories;
using Domain.Entities.Academics;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SubjectRepository : ISubjectRepository
{
    private readonly ApplicationDbContext _db;
    public SubjectRepository(ApplicationDbContext db) => _db = db;

    public async Task<bool> ExistsAsync(string code, CancellationToken ct) =>
        await _db.Subjects.AnyAsync(s => s.Code == code && s.SchoolId == null, ct);

    public Task<bool> ExistsForSchoolAsync(string code, Guid schoolId, Guid? exceptSubjectId = null, CancellationToken ct = default) =>
        _db.Subjects.AnyAsync(s => s.SchoolId == schoolId && s.Code == code && (!exceptSubjectId.HasValue || s.Id != exceptSubjectId.Value), ct);

    public async Task<bool> HasAcademicUsageAsync(Guid subjectId, Guid schoolId, CancellationToken ct = default)
    {
        if (await _db.TeachingAssignments.AnyAsync(x => x.SchoolId == schoolId && x.SubjectId == subjectId, ct))
            return true;
        if (await _db.Exams.AnyAsync(x => x.SchoolId == schoolId && x.SubjectId == subjectId, ct))
            return true;
        return await _db.ReportCardSubjectResults.AnyAsync(x => x.SubjectId == subjectId &&
            _db.ReportCards.Any(card => card.Id == x.ReportCardId && card.SchoolId == schoolId), ct);
    }

    public Task<Subject?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Subjects.FirstOrDefaultAsync(s => s.Id == id, ct);

    public void Add(Subject subject) => _db.Subjects.Add(subject);

    public void Remove(Subject subject) => _db.Subjects.Remove(subject);
}
