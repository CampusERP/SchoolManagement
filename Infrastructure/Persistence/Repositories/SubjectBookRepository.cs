using Application.Common.Interfaces.Repositories;
using Domain.Entities.Academics;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class SubjectBookRepository : ISubjectBookRepository
{
    private readonly ApplicationDbContext _db;
    public SubjectBookRepository(ApplicationDbContext db) => _db = db;

    public Task<SubjectBook?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.SubjectBooks.FirstOrDefaultAsync(book => book.Id == id, ct);

    public Task<SubjectBook?> GetByIdAndCurriculumAsync(Guid id, Guid curriculumSubjectId, CancellationToken ct = default) =>
        _db.SubjectBooks.FirstOrDefaultAsync(book => book.Id == id && book.CurriculumSubjectId == curriculumSubjectId, ct);

    public async Task<IReadOnlyList<SubjectBook>> GetByCurriculumSubjectAsync(Guid curriculumSubjectId, CancellationToken ct = default) =>
        await _db.SubjectBooks.AsNoTracking().Where(book => book.CurriculumSubjectId == curriculumSubjectId)
            .OrderBy(book => book.Title).ToListAsync(ct);

    public async Task AddAsync(SubjectBook book, CancellationToken ct = default) =>
        await _db.SubjectBooks.AddAsync(book, ct);

    public void Remove(SubjectBook book) => _db.SubjectBooks.Remove(book);
}
