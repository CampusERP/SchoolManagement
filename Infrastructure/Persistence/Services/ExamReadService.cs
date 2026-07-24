using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Application.Features.Exams.Queries;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Services;

public class ExamReadService : IExamReadService
{
    private readonly ApplicationDbContext _db;

    public ExamReadService(ApplicationDbContext db) => _db = db;

    public async Task<PagedResult<ExamListDto>> GetExamsAsync(
        Guid schoolId, Guid termId, PaginationParams p, CancellationToken ct = default)
    {
        var query = _db.Exams.AsNoTracking()
            .Where(e => e.SchoolId == schoolId && e.TermId == termId)
            .Join(_db.Subjects, e => e.SubjectId, s => s.Id, (e, s) => new { Exam = e, Subject = s })
            .Join(_db.Terms, x => x.Exam.TermId, t => t.Id, (x, t) => new { x.Exam, x.Subject, Term = t });

        var total = await query.CountAsync(ct);

        var allData = await query
            .OrderByDescending(x => x.Exam.CreatedAtUtc)
            .Skip(p.Skip)
            .Take(p.PageSize)
            .ToListAsync(ct);

        var items = new List<ExamListDto>();
        foreach (var x in allData)
        {
            var scheduleCount = await _db.ExamSchedules.AsNoTracking()
                .CountAsync(es => es.ExamId == x.Exam.Id, ct);

            items.Add(new ExamListDto(
                x.Exam.Id,
                x.Exam.Name,
                x.Subject.Name,
                x.Term.Name,
                x.Exam.MaxScore,
                x.Exam.IsLocked,
                scheduleCount));
        }

        return new PagedResult<ExamListDto>(items, total, p.Page, p.PageSize);
    }

    public async Task<PagedResult<ReportCardDto>> GetReportCardsAsync(
        Guid studentEnrollmentId, PaginationParams p, CancellationToken ct = default)
    {
        var query = _db.ReportCards.AsNoTracking()
            .Where(rc => rc.StudentEnrollmentId == studentEnrollmentId)
            .Join(_db.StudentEnrollments, rc => rc.StudentEnrollmentId, e => e.Id, (rc, e) => new { rc, e })
            .Join(_db.Students, x => x.e.StudentId, s => s.Id, (x, s) => new { x.rc, Student = s })
            .Join(_db.Terms, x => x.rc.TermId, t => t.Id, (x, t) => new { x.rc, x.Student, Term = t });

        var total = await query.CountAsync(ct);

        var allData = await query
            .OrderByDescending(x => x.rc.GeneratedAtUtc)
            .Skip(p.Skip)
            .Take(p.PageSize)
            .ToListAsync(ct);

        var items = new List<ReportCardDto>();
        foreach (var x in allData)
        {
            var subjects = await _db.ReportCardSubjectResults.AsNoTracking()
                .Where(s => s.ReportCardId == x.rc.Id)
                .Select(s => new ReportCardSubjectDto(
                    s.SubjectName, s.Score, s.MaxScore, s.Grade))
                .ToListAsync(ct);

            items.Add(new ReportCardDto(
                x.rc.Id,
                $"{x.Student.FirstName} {x.Student.LastName}",
                x.Term.Name,
                x.rc.OverallPercentage,
                x.rc.OverallGrade,
                x.rc.IsLocked,
                x.rc.GeneratedAtUtc,
                subjects));
        }

        return new PagedResult<ReportCardDto>(items, total, p.Page, p.PageSize);
    }

    public async Task<PagedResult<ExamResultDto>> GetStudentExamResultsAsync(
        Guid studentEnrollmentId, Guid? termId, PaginationParams p, CancellationToken ct = default)
    {
        var baseQuery = _db.ExamResults.AsNoTracking()
            .Where(er => er.StudentEnrollmentId == studentEnrollmentId)
            .Join(_db.Exams, er => er.ExamId, ex => ex.Id, (er, ex) => new { er, Exam = ex })
            .Join(_db.Subjects, x => x.Exam.SubjectId, s => s.Id, (x, s) => new { x.er, x.Exam, Subject = s })
            .AsQueryable();

        if (termId.HasValue)
            baseQuery = baseQuery.Where(x => x.Exam.TermId == termId.Value);

        var total = await baseQuery.CountAsync(ct);

        var allData = await baseQuery
            .OrderByDescending(x => x.er.Score)
            .Skip(p.Skip)
            .Take(p.PageSize)
            .ToListAsync(ct);

        var items = allData
            .Select(x => new ExamResultDto(
                x.Subject.Name,
                x.er.Score,
                x.Exam.MaxScore,
                x.Exam.MaxScore > 0 ? Math.Round(x.er.Score * 100m / x.Exam.MaxScore, 1) : 0,
                CalculateGrade(x.Exam.MaxScore > 0 ? x.er.Score * 100m / x.Exam.MaxScore : 0),
                x.er.Remarks))
            .ToList();

        return new PagedResult<ExamResultDto>(items, total, p.Page, p.PageSize);
    }

    public async Task<List<ClassExamResultDto>> GetClassExamResultsAsync(
        Guid examScheduleId, CancellationToken ct = default)
    {
        var allData = await _db.ExamResults.AsNoTracking()
            .Where(er => er.ExamScheduleId == examScheduleId)
            .Join(_db.Exams, er => er.ExamId, ex => ex.Id, (er, ex) => new { er, Exam = ex })
            .Join(_db.StudentEnrollments, x => x.er.StudentEnrollmentId, e => e.Id, (x, e) => new { x.er, x.Exam, Enrollment = e })
            .Join(_db.Students, x => x.Enrollment.StudentId, s => s.Id, (x, s) => new { x.er, x.Exam, x.Enrollment, Student = s })
            .OrderBy(x => x.Student.LastName)
            .ToListAsync(ct);

        return allData
            .Select(x => new ClassExamResultDto(
                x.Enrollment.Id,
                x.Student.StudentCode,
                x.Student.FirstName,
                x.Student.LastName,
                x.er.Score,
                x.Exam.MaxScore > 0 ? Math.Round(x.er.Score * 100m / x.Exam.MaxScore, 1) : 0,
                CalculateGrade(x.Exam.MaxScore > 0 ? x.er.Score * 100m / x.Exam.MaxScore : 0)))
            .ToList();
    }

    public async Task<List<ExamScheduleDto>> GetExamSchedulesAsync(
        Guid examId, CancellationToken ct = default)
    {
        return await _db.ExamSchedules.AsNoTracking()
            .Where(es => es.ExamId == examId)
            .Join(_db.ClassRooms, es => es.ClassRoomId, cr => cr.Id, (es, cr) => new { es, ClassRoom = cr })
            .Join(_db.Rooms, x => x.es.RoomId, r => r.Id, (x, r) => new ExamScheduleDto(
                x.es.Id, x.es.ClassRoomId, x.ClassRoom.Name, x.es.RoomId, r.Name, x.es.ExamDate))
            .ToListAsync(ct);
    }

    private static string CalculateGrade(decimal percentage) => percentage switch
    {
        >= 90 => "A+",
        >= 80 => "A",
        >= 70 => "B+",
        >= 60 => "B",
        >= 50 => "C+",
        >= 40 => "C",
        >= 30 => "D",
        _ => "F"
    };
}
