using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Features.People.Queries.GetStudents;
using Domain.Entities.People;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly ApplicationDbContext _context;

    public StudentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Students.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<bool> ExistsAsync(
        Guid schoolId,
        string studentCode,
        Guid? excludingStudentId = null,
        CancellationToken ct = default) =>
        await _context.Students.AnyAsync(
            s => s.SchoolId == schoolId
                && s.StudentCode == studentCode
                && (excludingStudentId == null || s.Id != excludingStudentId),
            ct);

    public async Task AddAsync(Student student, CancellationToken ct = default) =>
        await _context.Students.AddAsync(student, ct);

    public async Task RemoveAsync(Student student, CancellationToken ct = default)
    {
        student.IsDeleted = true;
        student.DeletedAtUtc = DateTime.UtcNow;
    }

    public async Task<PagedResult<StudentListDto>> GetStudentsAsync(
        Guid schoolId,
        Guid? academicYearId,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var studentsQuery = _context.Students
            .AsNoTracking()
            .Where(s => s.SchoolId == schoolId);

        var totalCount = await studentsQuery.CountAsync(ct);

        var itemsQuery = await studentsQuery
            .OrderBy(s => s.FirstName)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .Select(s => new {
                Student = s,
                Enrollment = _context.StudentEnrollments
                    .Where(e => e.StudentId == s.Id
                        && (academicYearId == null || e.AcademicYearId == academicYearId))
                    .Join(_context.ClassRooms, e => e.ClassRoomId, c => c.Id,
                        (e, c) => new { c.Name, e.Status })
                    .OrderByDescending(x => x.Status == EnrollmentStatus.Active)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        var items = itemsQuery.Select(x => new StudentListDto(
            x.Student.Id,
            x.Student.StudentCode,
            x.Student.FirstName,
            x.Student.LastName,
            null,
            x.Student.DateOfBirth,
            x.Enrollment?.Name ?? string.Empty,
            x.Enrollment?.Status.ToString() ?? string.Empty
        )).ToList();

        return new PagedResult<StudentListDto>(items, totalCount, pagination.Page, pagination.PageSize);
    }
}
