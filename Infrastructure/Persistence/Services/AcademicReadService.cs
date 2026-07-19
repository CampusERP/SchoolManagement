using Application.Common.Interfaces.Services;
using Application.Features.Academics.Queries.GetAcademicYears;
using Application.Features.Academics.Queries.GetClassRooms;
using Application.Features.Academics.Queries.GetGradeLevels;
using Application.Features.Academics.Queries.GetRooms;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Services;

public class AcademicReadService : IAcademicReadService
{
    private readonly ApplicationDbContext _db;

    public AcademicReadService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<AcademicYearDto>> GetAcademicYearsAsync(CancellationToken ct = default)
    {
        return await _db.AcademicYears.AsNoTracking()
            .OrderByDescending(y => y.StartDate)
            .Select(y => new AcademicYearDto(
                y.Id, y.Name, y.StartDate, y.EndDate, y.IsCurrent, y.Status.ToString(),
                y.Terms.OrderBy(t => t.Sequence)
                    .Select(t => new TermDto(t.Id, t.Name, t.Sequence, t.StartDate, t.EndDate)).ToList()))
            .ToListAsync(ct);
    }

    public async Task<List<GradeLevelDto>> GetGradeLevelsAsync(CancellationToken ct = default)
    {
        return await (from g in _db.GradeLevels.AsNoTracking()
                      join s in _db.EducationStages.AsNoTracking()
                          on g.EducationStageId equals s.Id
                      orderby g.Sequence
                      select new GradeLevelDto(
                          g.Id, g.Name, g.Sequence,
                          g.EducationStageId, s.Name,
                          _db.ClassRooms.Count(c => c.GradeLevelId == g.Id)))
                     .ToListAsync(ct);
    }

    public async Task<List<ClassRoomDetailDto>> GetClassRoomsAsync(Guid? academicYearId, Guid? gradeLevelId, CancellationToken ct = default)
    {
        var query = _db.ClassRooms.AsNoTracking().AsQueryable();

        if (academicYearId.HasValue)
            query = query.Where(c => c.AcademicYearId == academicYearId.Value);
        if (gradeLevelId.HasValue)
            query = query.Where(c => c.GradeLevelId == gradeLevelId.Value);

        return await (from c in query
                      join g in _db.GradeLevels.AsNoTracking() on c.GradeLevelId equals g.Id
                      join y in _db.AcademicYears.AsNoTracking() on c.AcademicYearId equals y.Id
                      orderby g.Sequence, c.Name
                      select new ClassRoomDetailDto(
                          c.Id, c.Name, g.Name, y.Name,
                          _db.StudentEnrollments.Count(e => e.ClassRoomId == c.Id && e.Status == Domain.Enums.EnrollmentStatus.Active),
                          _db.TeachingAssignments.Count(a => a.ClassRoomId == c.Id)))
                     .ToListAsync(ct);
    }

    public async Task<List<RoomDto>> GetRoomsAsync(CancellationToken ct = default)
    {
        return await _db.Rooms.AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RoomDto(r.Id, r.Name, r.Capacity))
            .ToListAsync(ct);
    }
}
