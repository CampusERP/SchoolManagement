using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.Academics;

namespace Infrastructure.Persistence.Repositories;

public class ClassRoomRepository : IClassRoomRepository
{
    private readonly ApplicationDbContext _context;

    public ClassRoomRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClassRoom?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.ClassRooms.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<bool> ExistsAsync(Guid schoolId, Guid gradeLevelId, Guid academicYearId, string name, CancellationToken ct = default) =>
        await _context.ClassRooms.AnyAsync(c =>
            c.SchoolId == schoolId && c.GradeLevelId == gradeLevelId &&
            c.AcademicYearId == academicYearId && c.Name == name, ct);

    public async Task AddAsync(ClassRoom classRoom, CancellationToken ct = default) =>
        await _context.ClassRooms.AddAsync(classRoom, ct);
}
