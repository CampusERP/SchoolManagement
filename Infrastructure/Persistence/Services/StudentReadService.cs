using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Application.Features.People.Queries.GetStudents;
using Application.Features.People.Queries.StudentDetails;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Services;

public class StudentReadService : IStudentReadService
{
    private readonly ApplicationDbContext _db;
    private readonly PlatformDbContext _platformDb;

    public StudentReadService(ApplicationDbContext db, PlatformDbContext platformDb)
    {
        _db = db;
        _platformDb = platformDb;
    }

    public async Task<PagedResult<StudentListDto>> GetStudentsAsync(
        Guid schoolId,
        string? searchTerm,
        Guid? gradeLevelId,
        Guid? classRoomId,
        PaginationParams pagination,
        CancellationToken cancellationToken)
    {
        var query = _db.Students.AsNoTracking().Where(s => s.SchoolId == schoolId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim().ToLower();

            query = query.Where(s =>
                s.FirstName.ToLower().Contains(searchTerm) ||
                s.LastName.ToLower().Contains(searchTerm) ||
                s.StudentCode.ToLower().Contains(searchTerm));
        }

        if (gradeLevelId.HasValue || classRoomId.HasValue)
        {
            query = query.Where(s => _db.StudentEnrollments.Any(e => 
                e.StudentId == s.Id && 
                e.Status == EnrollmentStatus.Active &&
                (!gradeLevelId.HasValue || _db.ClassRooms.Any(c => c.Id == e.ClassRoomId && c.GradeLevelId == gradeLevelId.Value)) &&
                (!classRoomId.HasValue || e.ClassRoomId == classRoomId.Value)));
        }

        var total = await query.CountAsync(cancellationToken);

        var itemsQuery = await query
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .Select(s => new {
                Student = s,
                Enrollment = _db.StudentEnrollments
                    .Where(e => e.StudentId == s.Id && e.Status == EnrollmentStatus.Active)
                    .Join(_db.ClassRooms, e => e.ClassRoomId, c => c.Id, (e, c) => new { c.Name, e.Status })
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var students = itemsQuery.Select(x => new StudentListDto(
            x.Student.Id,
            x.Student.StudentCode,
            x.Student.FirstName,
            x.Student.LastName,
            null,
            x.Student.DateOfBirth,
            x.Enrollment?.Name,
            x.Enrollment?.Status.ToString()
        )).ToList();

        var studentUserIds = students
            .Where(s => itemsQuery.Any(x => x.Student.Id == s.Id && x.Student.ApplicationUserId != null))
            .Select(s => s.Id)
            .ToList();

        var studentApplicationUserIds = await _db.Students
            .AsNoTracking()
            .Where(s => studentUserIds.Contains(s.Id) && s.ApplicationUserId != null)
            .Select(s => new { s.Id, s.ApplicationUserId })
            .ToListAsync(cancellationToken);

        var userIdToStudentId = studentApplicationUserIds.ToDictionary(x => x.ApplicationUserId!.Value, x => x.Id);
        var userIds = studentApplicationUserIds.Select(x => x.ApplicationUserId!.Value).ToList();
        var emails = userIds.Count > 0
            ? await _platformDb.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Email })
                .ToDictionaryAsync(u => u.Id, u => u.Email, cancellationToken)
            : new Dictionary<Guid, string?>();

        var finalStudents = students.Select(s => {
            var appUserId = studentApplicationUserIds.FirstOrDefault(x => x.Id == s.Id)?.ApplicationUserId;
            var email = appUserId.HasValue ? emails.GetValueOrDefault(appUserId.Value) : null;
            return new StudentListDto(s.Id, s.StudentCode, s.FirstName, s.LastName, email, s.DateOfBirth, s.CurrentClass, s.EnrollmentStatus);
        }).ToList();

        return new PagedResult<StudentListDto>(
            finalStudents,
            total,
            pagination.Page,
            pagination.PageSize);
    }

    public async Task<StudentDetailDto?> GetStudentDetailsAsync(Guid schoolId, Guid studentId, CancellationToken cancellationToken)
    {
        var student = await _db.Students.AsNoTracking()
            .Where(s => s.SchoolId == schoolId && s.Id == studentId)
            .Select(s => new {
                Student = s,
                Enrollments = _db.StudentEnrollments
                    .Where(e => e.StudentId == s.Id)
                    .Join(_db.AcademicYears, e => e.AcademicYearId, a => a.Id, (e, a) => new { e, a })
                    .Join(_db.ClassRooms, ea => ea.e.ClassRoomId, c => c.Id, (ea, c) => new { ea.e, ea.a, c })
                    .Join(_db.GradeLevels, eac => eac.c.GradeLevelId, g => g.Id, (eac, g) => new StudentEnrollmentSummaryDto(
                        eac.e.Id, eac.a.Name, eac.c.Name, g.Name, eac.e.Status.ToString()))
                    .ToList(),
                Guardians = _db.StudentGuardians
                    .Where(g => g.StudentId == s.Id)
                    .Join(_db.Parents, g => g.ParentId, p => p.Id, (g, p) => new GuardianSummaryDto(
                        p.Id, p.FirstName, p.LastName, g.RelationshipType.ToString(), g.IsPrimaryContact))
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (student == null) return null;

        return new StudentDetailDto(
            student.Student.Id,
            student.Student.StudentCode,
            student.Student.FirstName,
            student.Student.LastName,
            student.Student.DateOfBirth,
            student.Student.NationalId,
            student.Student.ApplicationUserId != null,
            student.Enrollments,
            student.Guardians
        );
    }

    public async Task<StudentDetailDto?> GetMyProfileAsync(Guid schoolId, Guid userId, CancellationToken cancellationToken)
    {
        var student = await _db.Students.AsNoTracking()
            .Where(s => s.SchoolId == schoolId && s.ApplicationUserId == userId)
            .Select(s => new {
                Student = s,
                Enrollments = _db.StudentEnrollments
                    .Where(e => e.StudentId == s.Id)
                    .Join(_db.AcademicYears, e => e.AcademicYearId, a => a.Id, (e, a) => new { e, a })
                    .Join(_db.ClassRooms, ea => ea.e.ClassRoomId, c => c.Id, (ea, c) => new { ea.e, ea.a, c })
                    .Join(_db.GradeLevels, eac => eac.c.GradeLevelId, g => g.Id, (eac, g) => new StudentEnrollmentSummaryDto(
                        eac.e.Id, eac.a.Name, eac.c.Name, g.Name, eac.e.Status.ToString()))
                    .ToList(),
                Guardians = _db.StudentGuardians
                    .Where(g => g.StudentId == s.Id)
                    .Join(_db.Parents, g => g.ParentId, p => p.Id, (g, p) => new GuardianSummaryDto(
                        p.Id, p.FirstName, p.LastName, g.RelationshipType.ToString(), g.IsPrimaryContact))
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (student == null) return null;

        return new StudentDetailDto(
            student.Student.Id,
            student.Student.StudentCode,
            student.Student.FirstName,
            student.Student.LastName,
            student.Student.DateOfBirth,
            student.Student.NationalId,
            student.Student.ApplicationUserId != null,
            student.Enrollments,
            student.Guardians
        );
    }
}