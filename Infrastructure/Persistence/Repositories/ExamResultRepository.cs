using Application.Common.Interfaces.Repositories;
using Domain.Entities.Exams;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ExamResultRepository : IExamResultRepository
{
    private readonly ApplicationDbContext _db;
    public ExamResultRepository(ApplicationDbContext db) => _db = db;

    public async Task<List<ExamResult>> GetResultsForReportCardAsync(Guid enrollmentId, Guid termId, CancellationToken ct = default) =>
        await _db.ExamResults
            .Where(er => er.StudentEnrollmentId == enrollmentId
                && _db.Exams.Any(e => e.Id == er.ExamId && e.TermId == termId))
            .ToListAsync(ct);
}
