using Application.Common.Interfaces.Repositories;
using Domain.Entities.Exams;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ReportCardRepository : IReportCardRepository
{
    private readonly ApplicationDbContext _db;
    public ReportCardRepository(ApplicationDbContext db) => _db = db;

    public async Task<ReportCard?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.ReportCards.FindAsync(new object[] { id }, ct);

    public async Task<bool> ExistsAsync(Guid studentEnrollmentId, Guid termId, CancellationToken ct = default) =>
        await _db.ReportCards.AnyAsync(r => r.StudentEnrollmentId == studentEnrollmentId && r.TermId == termId, ct);

    public async Task AddAsync(ReportCard card, CancellationToken ct = default) =>
        await _db.ReportCards.AddAsync(card, ct);
}
