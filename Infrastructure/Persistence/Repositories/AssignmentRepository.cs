using Application.Common.Interfaces.Repositories;
using Domain.Entities.Assignments;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly ApplicationDbContext _db;
    public AssignmentRepository(ApplicationDbContext db) => _db = db;

    public async Task<Assignment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Assignments.FindAsync(new object[] { id }, ct);

    public async Task AddAsync(Assignment assignment, CancellationToken ct = default) =>
        await _db.Assignments.AddAsync(assignment, ct);
}
