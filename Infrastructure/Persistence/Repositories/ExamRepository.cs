using Application.Common.Interfaces.Repositories;
using Domain.Entities.Exams;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ExamRepository : IExamRepository
{
    private readonly ApplicationDbContext _db;
    public ExamRepository(ApplicationDbContext db) => _db = db;

    public async Task<Exam?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Exams
            .Include(e => e.Schedules)
            .Include(e => e.Results)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task AddAsync(Exam exam, CancellationToken ct = default) =>
        await _db.Exams.AddAsync(exam, ct);

    /// <summary>
    /// Persists the schedule just added to an existing exam. EF can otherwise
    /// classify a new child with an assigned Guid key as Modified, causing an
    /// UPDATE with a RowVersion predicate instead of the required INSERT.
    /// </summary>
    public async Task<Guid> AddScheduleAsync(Exam exam, CancellationToken ct = default)
    {
        var schedule = exam.Schedules.Last();
        _db.Entry(exam).State = EntityState.Unchanged;
        _db.Entry(schedule).State = EntityState.Added;
        await _db.SaveChangesAsync(ct);
        return schedule.Id;
    }

    public async Task SaveResultsAsync(Exam exam, IReadOnlyCollection<ExamResult> newResults,
        CancellationToken ct = default)
    {
        _db.Entry(exam).State = EntityState.Unchanged;
        foreach (var result in newResults)
            _db.Entry(result).State = EntityState.Added;

        await _db.SaveChangesAsync(ct);
    }
}
