using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.Academics;

namespace Infrastructure.Persistence.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly ApplicationDbContext _context;

    public RoomRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Room?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<bool> ExistsAsync(Guid schoolId, string name, CancellationToken ct = default) =>
        await _context.Rooms.AnyAsync(r => r.SchoolId == schoolId && r.Name == name, ct);

    public async Task AddAsync(Room room, CancellationToken ct = default) =>
        await _context.Rooms.AddAsync(room, ct);
}
