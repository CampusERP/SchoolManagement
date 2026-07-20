using Application.Common.Interfaces.Repositories;
using Domain.Entities.Academics;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SubjectRepository : ISubjectRepository
{
    private readonly ApplicationDbContext _db;
    public SubjectRepository(ApplicationDbContext db) => _db = db;

    public async Task<bool> ExistsAsync(string code, CancellationToken ct) =>
        await _db.Subjects.AnyAsync(s => s.Code == code, ct);

    public void Add(Subject subject) => _db.Subjects.Add(subject);
}
