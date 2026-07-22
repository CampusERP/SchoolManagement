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
}
