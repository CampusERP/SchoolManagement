using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Application.Features.Portal.Queries.GetParentDashboard;
using Application.Features.Portal.Queries.GetParentChildAttendance;
using Application.Features.Portal.Queries.GetParentChildProfile;
using Application.Features.Portal.Queries.GetStudentClasses;
using Application.Features.Portal.Queries.GetStudentDashboard;
using Application.Features.Portal.Queries.GetTeacherDashboard;
using Application.Features.Portal.Queries.GetTeacherSchedule;
using Application.Features.Portal.Queries.Shared;
using Domain.Entities.Notifications;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Services;

public class PortalReadService : IPortalReadService
{
    private readonly ApplicationDbContext _db;
    private readonly PlatformDbContext _platformDb;

    public PortalReadService(ApplicationDbContext db, PlatformDbContext platformDb)
    {
        _db = db;
        _platformDb = platformDb;
    }

    // ════════════════════════════════════════════════════════
    //  TEACHER PORTAL
    // ════════════════════════════════════════════════════════

    public async Task<TeacherDashboardDto> GetTeacherDashboardAsync(
        Guid schoolId, Guid userId, CancellationToken ct = default)
    {
        var today = (DayOfWeekEnum)(int)DateTime.UtcNow.DayOfWeek;

        var teacher = await _db.Teachers.AsNoTracking()
            .Where(t => t.SchoolId == schoolId && t.ApplicationUserId == userId)
            .FirstOrDefaultAsync(ct);

        if (teacher is null)
            return new TeacherDashboardDto(Guid.Empty, null, 0, 0, 0, 0, 0,
                new List<TeacherDashboardScheduleSlot>(),
                new List<TeacherDashboardClass>(),
                new List<TeacherDashboardAnnouncement>());

        var teachingAssignments = await _db.TeachingAssignments.AsNoTracking()
            .Where(ta => ta.TeacherId == teacher.Id)
            .Join(_db.ClassRooms, ta => ta.ClassRoomId, c => c.Id, (ta, c) => new { ta, ClassRoom = c })
            .Join(_db.Subjects, x => x.ta.SubjectId, s => s.Id, (x, s) => new { x.ta, x.ClassRoom, Subject = s })
            .Join(_db.GradeLevels, x => x.ClassRoom.GradeLevelId, g => g.Id, (x, g) => new { x.ta, x.ClassRoom, x.Subject, GradeLevel = g })
            .ToListAsync(ct);

        var teacherFullName = $"{teacher.FirstName} {teacher.LastName}";
        var distinctClassRoomIds = teachingAssignments.Select(x => x.ClassRoom.Id).Distinct().ToList();
        var distinctTeachingAssignmentIds = teachingAssignments.Select(x => x.ta.Id).Distinct().ToList();

        int totalStudents = 0;
        var myClasses = new List<TeacherDashboardClass>();
        foreach (var ta in teachingAssignments)
        {
            var studentCount = await _db.StudentEnrollments.AsNoTracking()
                .CountAsync(e => e.ClassRoomId == ta.ClassRoom.Id && e.Status == EnrollmentStatus.Active, ct);

            totalStudents += studentCount;

            myClasses.Add(new TeacherDashboardClass(
                ta.ta.Id,
                ta.ClassRoom.Id,
                ta.ClassRoom.Name,
                ta.Subject.Name,
                teacherFullName,
                studentCount,
                ta.GradeLevel.Name));
        }

        myClasses = myClasses
            .GroupBy(c => c.ClassRoomId)
            .Select(g => g.First())
            .ToList();

        var todayScheduleData = await _db.TeachingAssignments.AsNoTracking()
            .Where(ta => ta.TeacherId == teacher.Id)
            .Join(_db.ClassSchedules, ta => ta.Id, cs => cs.TeachingAssignmentId, (ta, cs) => new { ta, cs })
            .Where(x => x.cs.DayOfWeek == today)
            .Join(_db.Subjects, x => x.ta.SubjectId, s => s.Id, (x, s) => new { x.ta, x.cs, Subject = s })
            .Join(_db.ClassRooms, x => x.ta.ClassRoomId, c => c.Id, (x, c) => new { x.ta, x.cs, x.Subject, ClassRoom = c })
            .Join(_db.Rooms, x => x.cs.RoomId, r => r.Id, (x, r) => new { x.ta, x.cs, x.Subject, x.ClassRoom, Room = r })
            .OrderBy(x => x.cs.StartTime)
            .ToListAsync(ct);

        var now = TimeOnly.FromDateTime(DateTime.UtcNow);

        var todaySchedule = todayScheduleData
            .Select(x => new TeacherDashboardScheduleSlot(
                x.cs.Id,
                x.ta.Id,
                x.ClassRoom.Name,
                x.Subject.Name,
                teacherFullName,
                x.Room.Name,
                x.cs.StartTime,
                x.cs.EndTime,
                now < TimeOnly.FromTimeSpan(x.cs.StartTime) ? "upcoming" :
                now >= TimeOnly.FromTimeSpan(x.cs.StartTime) && now < TimeOnly.FromTimeSpan(x.cs.EndTime) ? "in_progress" : "completed"))
            .ToList();

        var todayLessonCount = todaySchedule.Count;

        var todayClassScheduleIds = todayScheduleData.Select(x => x.cs.Id).ToList();

        var pendingAttendance = await _db.AttendanceSessions.AsNoTracking()
            .CountAsync(s => todayClassScheduleIds.Contains(s.ClassScheduleId)
                && s.Date == DateOnly.FromDateTime(DateTime.UtcNow)
                && !s.IsLocked, ct);

        var pendingAssignments = 0;
        foreach (var taId in distinctTeachingAssignmentIds)
        {
            var assignments = await _db.Assignments.AsNoTracking()
                .Where(a => a.TeachingAssignmentId == taId)
                .ToListAsync(ct);

            foreach (var assignment in assignments)
            {
                var hasUngraded = await _db.AssignmentSubmissions.AsNoTracking()
                    .AnyAsync(s => s.AssignmentId == assignment.Id
                        && s.Status != SubmissionStatus.Graded, ct);

                if (hasUngraded)
                    pendingAssignments++;
            }
        }

        var announcements = await _db.NotificationBatches.AsNoTracking()
            .Where(b => b.SchoolId == schoolId)
            .OrderByDescending(b => b.CreatedAtUtc)
            .Take(5)
            .Select(b => new TeacherDashboardAnnouncement(
                b.Id,
                b.Subject,
                b.Body,
                "System",
                b.CreatedAtUtc))
            .ToListAsync(ct);

        var currentTermId = teachingAssignments.FirstOrDefault()?.ta.TermId;

        return new TeacherDashboardDto(
            teacher.Id,
            currentTermId,
            myClasses.Count,
            totalStudents,
            todayLessonCount,
            pendingAttendance,
            pendingAssignments,
            todaySchedule,
            myClasses,
            announcements);
    }

    public async Task<List<TeacherScheduleSlotDto>> GetTeacherScheduleAsync(
        Guid teacherId, Guid termId, CancellationToken ct = default)
    {
        return await _db.TeachingAssignments.AsNoTracking()
            .Where(ta => ta.TeacherId == teacherId && ta.TermId == termId)
            .Join(_db.ClassSchedules,
                ta => ta.Id, cs => cs.TeachingAssignmentId,
                (ta, cs) => new { ta, cs })
            .Join(_db.Subjects,
                x => x.ta.SubjectId, s => s.Id,
                (x, s) => new { x.ta, x.cs, Subject = s })
            .Join(_db.ClassRooms,
                x => x.ta.ClassRoomId, c => c.Id,
                (x, c) => new { x.ta, x.cs, x.Subject, ClassRoom = c })
            .Join(_db.GradeLevels,
                x => x.ClassRoom.GradeLevelId, g => g.Id,
                (x, g) => new { x.ta, x.cs, x.Subject, x.ClassRoom, GradeLevel = g })
            .OrderBy(x => x.cs.DayOfWeek)
            .ThenBy(x => x.cs.StartTime)
            .Join(_db.Rooms,
                x => x.cs.RoomId, r => r.Id,
                (x, r) => new TeacherScheduleSlotDto(
                    x.ta.Id,
                    x.Subject.Name,
                    x.ClassRoom.Name,
                    x.GradeLevel.Name,
                    r.Name,
                    x.cs.DayOfWeek,
                    x.cs.StartTime,
                    x.cs.EndTime))
            .ToListAsync(ct);
    }

    public async Task<PagedResult<RosterStudentDto>> GetClassRoomRosterAsync(
        Guid classRoomId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.StudentEnrollments.AsNoTracking()
            .Where(e => e.ClassRoomId == classRoomId && e.Status == EnrollmentStatus.Active)
            .Join(_db.Students,
                e => e.StudentId, s => s.Id,
                (e, s) => new { e, Student = s });

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(x => x.Student.LastName)
            .ThenBy(x => x.Student.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new RosterStudentDto(
                x.e.Id,
                x.Student.Id,
                x.Student.StudentCode,
                x.Student.FirstName,
                x.Student.LastName,
                x.e.Status.ToString()))
            .ToListAsync(ct);

        return new PagedResult<RosterStudentDto>(items, total, page, pageSize);
    }

    // ════════════════════════════════════════════════════════
    //  STUDENT PORTAL
    // ════════════════════════════════════════════════════════

    public async Task<List<StudentScheduleSlotDto>> GetStudentScheduleAsync(
        Guid enrollmentId, Guid termId, CancellationToken ct = default)
    {
        var enrollment = await _db.StudentEnrollments.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == enrollmentId, ct);

        if (enrollment is null) return new List<StudentScheduleSlotDto>();

        return await _db.TeachingAssignments.AsNoTracking()
            .Where(ta => ta.ClassRoomId == enrollment.ClassRoomId && ta.TermId == termId)
            .Join(_db.ClassSchedules,
                ta => ta.Id, cs => cs.TeachingAssignmentId,
                (ta, cs) => new { ta, cs })
            .Join(_db.Subjects,
                x => x.ta.SubjectId, s => s.Id,
                (x, s) => new { x.ta, x.cs, Subject = s })
            .Join(_db.Teachers,
                x => x.ta.TeacherId, t => t.Id,
                (x, t) => new { x.ta, x.cs, x.Subject, Teacher = t })
            .OrderBy(x => x.cs.DayOfWeek)
            .ThenBy(x => x.cs.StartTime)
            .Join(_db.Rooms,
                x => x.cs.RoomId, r => r.Id,
                (x, r) => new StudentScheduleSlotDto(
                    x.Subject.Name,
                    x.Teacher.FirstName,
                    x.Teacher.LastName,
                    r.Name,
                    x.cs.DayOfWeek,
                    x.cs.StartTime,
                    x.cs.EndTime))
            .ToListAsync(ct);
    }

    public async Task<StudentSummaryDto?> GetStudentSummaryAsync(
        Guid enrollmentId, CancellationToken ct = default)
    {
        return await _db.StudentEnrollments.AsNoTracking()
            .Where(e => e.Id == enrollmentId)
            .Join(_db.Students, e => e.StudentId, s => s.Id, (e, s) => new { e, Student = s })
            .Join(_db.ClassRooms, x => x.e.ClassRoomId, c => c.Id, (x, c) => new { x.e, x.Student, ClassRoom = c })
            .Join(_db.GradeLevels, x => x.ClassRoom.GradeLevelId, g => g.Id, (x, g) => new { x.e, x.Student, x.ClassRoom, GradeLevel = g })
            .Join(_db.AcademicYears, x => x.e.AcademicYearId, a => a.Id, (x, a) => new { x.e, x.Student, x.ClassRoom, x.GradeLevel, AcademicYear = a })
            .Select(x => new StudentSummaryDto(
                x.Student.FirstName + " " + x.Student.LastName,
                x.ClassRoom.Name,
                x.GradeLevel.Name,
                x.AcademicYear.Name,
                _db.AttendanceRecords
                    .Where(ar => ar.StudentEnrollmentId == x.e.Id)
                    .Count(ar => ar.Status == AttendanceStatus.Present) * 100m /
                (decimal)Math.Max(1, _db.AttendanceRecords.Count(ar => ar.StudentEnrollmentId == x.e.Id)),
                _db.Assignments
                    .Where(a => _db.TeachingAssignments
                        .Where(ta => ta.ClassRoomId == x.e.ClassRoomId)
                        .Select(ta => ta.Id)
                        .Contains(a.TeachingAssignmentId))
                    .Count(a => !_db.AssignmentSubmissions
                        .Any(s => s.AssignmentId == a.Id && s.StudentEnrollmentId == x.e.Id))))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<StudentDashboardDto?> GetStudentDashboardAsync(
        Guid schoolId, Guid userId, Guid enrollmentId, CancellationToken ct = default)
    {
        var enrollment = await _db.StudentEnrollments.AsNoTracking()
            .Where(e => e.Id == enrollmentId && e.SchoolId == schoolId)
            .Join(_db.Students, e => e.StudentId, s => s.Id, (e, s) => new { e, Student = s })
            .Join(_db.ClassRooms, x => x.e.ClassRoomId, c => c.Id, (x, c) => new { x.e, x.Student, ClassRoom = c })
            .Join(_db.GradeLevels, x => x.ClassRoom.GradeLevelId, g => g.Id, (x, g) => new { x.e, x.Student, x.ClassRoom, GradeLevel = g })
            .Join(_db.AcademicYears, x => x.e.AcademicYearId, a => a.Id, (x, a) => new { x.e, x.Student, x.ClassRoom, x.GradeLevel, AcademicYear = a })
            .FirstOrDefaultAsync(ct);

        if (enrollment is null) return null;
        if (enrollment.Student.ApplicationUserId != userId) return null;

        var today = (DayOfWeekEnum)(int)DateTime.UtcNow.DayOfWeek;
        var termId = await _db.TeachingAssignments.AsNoTracking()
            .Where(ta => ta.ClassRoomId == enrollment.e.ClassRoomId)
            .Select(ta => ta.TermId)
            .FirstOrDefaultAsync(ct);

        var todaySchedule = await _db.TeachingAssignments.AsNoTracking()
            .Where(ta => ta.ClassRoomId == enrollment.e.ClassRoomId && ta.TermId == termId)
            .Join(_db.ClassSchedules,
                ta => ta.Id, cs => cs.TeachingAssignmentId,
                (ta, cs) => new { ta, cs })
            .Where(x => x.cs.DayOfWeek == today)
            .Join(_db.Subjects, x => x.ta.SubjectId, s => s.Id, (x, s) => new { x.ta, x.cs, Subject = s })
            .Join(_db.Teachers, x => x.ta.TeacherId, t => t.Id, (x, t) => new { x.cs, x.Subject, Teacher = t })
            .OrderBy(x => x.cs.StartTime)
            .Join(_db.Rooms, x => x.cs.RoomId, r => r.Id, (x, r) => new StudentDashboardScheduleSlot(
                x.Subject.Name,
                x.Teacher.FirstName + " " + x.Teacher.LastName,
                r.Name,
                x.cs.StartTime,
                x.cs.EndTime))
            .ToListAsync(ct);

        var teachingAssignmentIds = await _db.TeachingAssignments.AsNoTracking()
            .Where(ta => ta.ClassRoomId == enrollment.e.ClassRoomId)
            .Select(ta => ta.Id)
            .ToListAsync(ct);

        var assignmentQuery = _db.Assignments.AsNoTracking()
            .Where(a => teachingAssignmentIds.Contains(a.TeachingAssignmentId) && a.DueDate > DateTime.UtcNow)
            .Join(_db.TeachingAssignments, a => a.TeachingAssignmentId, ta => ta.Id, (a, ta) => new { a, ta })
            .Join(_db.Subjects, x => x.ta.SubjectId, s => s.Id, (x, s) => new { x.a, Subject = s });

        var allAssignments = await assignmentQuery
            .OrderBy(x => x.a.DueDate)
            .Take(10)
            .ToListAsync(ct);

        var enrollmentIdVal = enrollment.e.Id;
        var upcomingAssignments = new List<StudentDashboardAssignment>();
        foreach (var item in allAssignments)
        {
            var submission = await _db.AssignmentSubmissions.AsNoTracking()
                .FirstOrDefaultAsync(s => s.AssignmentId == item.a.Id && s.StudentEnrollmentId == enrollmentIdVal, ct);

            upcomingAssignments.Add(new StudentDashboardAssignment(
                item.a.Id,
                item.a.Title,
                item.Subject.Name,
                item.a.DueDate,
                item.a.MaxScore,
                submission?.Status,
                submission?.Grade));
        }

        var attendanceRecords = await _db.AttendanceRecords.AsNoTracking()
            .Where(ar => ar.StudentEnrollmentId == enrollmentIdVal)
            .ToListAsync(ct);

        var totalDays = attendanceRecords.Count;
        var presentDays = attendanceRecords.Count(ar => ar.Status == AttendanceStatus.Present);
        var attendancePct = totalDays > 0 ? Math.Round(presentDays * 100m / totalDays, 1) : 100m;

        var recentGradeData = await _db.ExamResults.AsNoTracking()
            .Where(er => er.StudentEnrollmentId == enrollmentIdVal)
            .Join(_db.Exams, er => er.ExamId, ex => ex.Id, (er, ex) => new { er, Exam = ex })
            .Join(_db.Subjects, x => x.Exam.SubjectId, s => s.Id, (x, s) => new { x.er, x.Exam, Subject = s })
            .OrderByDescending(x => x.er.Score)
            .Take(10)
            .ToListAsync(ct);

        var recentGrades = recentGradeData
            .Select(x => new StudentDashboardExamResult(
                x.Subject.Name,
                x.Exam.Name,
                x.er.Score,
                x.Exam.MaxScore,
                x.Exam.MaxScore > 0 ? Math.Round(x.er.Score * 100m / x.Exam.MaxScore, 1) : 0,
                CalculateGrade(x.Exam.MaxScore > 0 ? x.er.Score * 100m / x.Exam.MaxScore : 0)))
            .ToList();

        var unreadCount = await _db.Notifications.AsNoTracking()
            .Where(n => n.RecipientUserId == userId && n.Status == NotificationStatus.Delivered)
            .CountAsync(ct);

        var latestReportCard = await _db.ReportCards.AsNoTracking()
            .Where(rc => rc.StudentEnrollmentId == enrollmentIdVal)
            .OrderByDescending(rc => rc.GeneratedAtUtc)
            .Select(rc => new { rc.OverallPercentage, rc.OverallGrade })
            .FirstOrDefaultAsync(ct);

        return new StudentDashboardDto(
            enrollment.Student.Id,
            enrollment.Student.StudentCode,
            enrollment.Student.FirstName,
            enrollment.Student.LastName,
            enrollment.AcademicYear.Name,
            enrollment.ClassRoom.Name,
            enrollment.GradeLevel.Name,
            enrollment.e.Id,
            todaySchedule,
            upcomingAssignments,
            totalDays,
            presentDays,
            attendancePct,
            recentGrades,
            unreadCount,
            latestReportCard != null ? latestReportCard.OverallPercentage : null,
            latestReportCard != null ? latestReportCard.OverallGrade : null);
    }

    public async Task<List<StudentClassDto>> GetStudentClassesAsync(
        Guid schoolId, Guid userId, Guid enrollmentId, CancellationToken ct = default)
    {
        var enrollment = await _db.StudentEnrollments.AsNoTracking()
            .Where(e => e.Id == enrollmentId && e.SchoolId == schoolId)
            .Join(_db.Students, e => e.StudentId, s => s.Id, (e, s) => new { e, Student = s })
            .FirstOrDefaultAsync(ct);

        if (enrollment is null || enrollment.Student.ApplicationUserId != userId)
            return new List<StudentClassDto>();

        var teachingAssignments = await _db.TeachingAssignments.AsNoTracking()
            .Where(ta => ta.ClassRoomId == enrollment.e.ClassRoomId)
            .Join(_db.Subjects, ta => ta.SubjectId, s => s.Id, (ta, s) => new { ta, Subject = s })
            .Join(_db.Teachers, x => x.ta.TeacherId, t => t.Id, (x, t) => new { x.ta, x.Subject, Teacher = t })
            .Join(_db.ClassRooms, x => x.ta.ClassRoomId, c => c.Id, (x, c) => new { x.ta, x.Subject, x.Teacher, ClassRoom = c })
            .ToListAsync(ct);

        var result = new List<StudentClassDto>();
        foreach (var ta in teachingAssignments)
        {
            var slots = await _db.ClassSchedules.AsNoTracking()
                .Where(cs => cs.TeachingAssignmentId == ta.ta.Id)
                .Join(_db.Rooms, cs => cs.RoomId, r => r.Id, (cs, r) => new { cs, Room = r })
                .OrderBy(x => x.cs.DayOfWeek)
                .ThenBy(x => x.cs.StartTime)
                .Select(x => new StudentClassScheduleSlot(
                    x.cs.DayOfWeek, x.cs.StartTime, x.cs.EndTime, x.Room.Name))
                .ToListAsync(ct);

            result.Add(new StudentClassDto(
                ta.ta.Id,
                ta.Subject.Name,
                ta.Subject.Code,
                ta.Teacher.FirstName,
                ta.Teacher.LastName,
                ta.ClassRoom.Name,
                slots));
        }

        return result;
    }

    public async Task<List<PortalNotificationDto>> GetStudentNotificationsAsync(
        Guid userId, int limit, CancellationToken ct = default)
    {
        return await _db.Notifications.AsNoTracking()
            .Where(n => n.RecipientUserId == userId)
            .Join(_db.NotificationBatches,
                n => n.NotificationBatchId, b => b.Id,
                (n, b) => new { n, Batch = b })
            .OrderByDescending(x => x.Batch.CreatedAtUtc)
            .Take(limit)
            .Select(x => new PortalNotificationDto(
                x.n.Id,
                x.Batch.Subject,
                x.Batch.Body,
                x.n.Status.ToString(),
                x.Batch.CreatedAtUtc,
                x.n.ReadAtUtc))
            .ToListAsync(ct);
    }

    // ════════════════════════════════════════════════════════
    //  PARENT PORTAL
    // ════════════════════════════════════════════════════════

    public async Task<ParentDashboardDto> GetParentDashboardAsync(
        Guid schoolId, Guid userId, CancellationToken ct = default)
    {
        var parentId = await _db.Parents.AsNoTracking()
            .Where(p => p.SchoolId == schoolId && p.ApplicationUserId == userId)
            .Select(p => p.Id)
            .FirstOrDefaultAsync(ct);

        if (parentId == Guid.Empty)
            return new ParentDashboardDto(new List<ParentChildDashboardDto>(), 0, 0, 0);

        var childEnrollments = await _db.StudentGuardians.AsNoTracking()
            .Where(g => g.ParentId == parentId)
            .Join(_db.Students, g => g.StudentId, s => s.Id, (g, s) => new { g, Student = s })
            .Join(_db.StudentEnrollments,
                x => x.Student.Id, e => e.StudentId,
                (x, e) => new { x.g, x.Student, Enrollment = e })
            .Where(x => x.Enrollment.Status == EnrollmentStatus.Active)
            .Join(_db.ClassRooms,
                x => x.Enrollment.ClassRoomId, c => c.Id,
                (x, c) => new { x.g, x.Student, x.Enrollment, ClassRoom = c })
            .ToListAsync(ct);

        var children = new List<ParentChildDashboardDto>();
        foreach (var ce in childEnrollments)
        {
            var enrollmentId = ce.Enrollment.Id;

            var attendanceRecords = await _db.AttendanceRecords.AsNoTracking()
                .Where(ar => ar.StudentEnrollmentId == enrollmentId)
                .ToListAsync(ct);

            var totalDays = attendanceRecords.Count;
            var presentDays = attendanceRecords.Count(ar => ar.Status == AttendanceStatus.Present);
            var attendancePct = totalDays > 0 ? Math.Round(presentDays * 100m / totalDays, 1) : 100m;

            var gradeData = await _db.ExamResults.AsNoTracking()
                .Where(er => er.StudentEnrollmentId == enrollmentId)
                .Join(_db.Exams, er => er.ExamId, ex => ex.Id, (er, ex) => new { er, Exam = ex })
                .Join(_db.Subjects, x => x.Exam.SubjectId, s => s.Id, (x, s) => new { x.er, x.Exam, Subject = s })
                .OrderByDescending(x => x.er.Score)
                .Take(5)
                .ToListAsync(ct);

            var recentGrades = gradeData
                .Select(x => new ParentChildRecentGrade(
                    x.Subject.Name,
                    x.Exam.Name,
                    x.er.Score,
                    x.Exam.MaxScore,
                    x.Exam.MaxScore > 0 ? Math.Round(x.er.Score * 100m / x.Exam.MaxScore, 1) : 0,
                    CalculateGrade(x.Exam.MaxScore > 0 ? x.er.Score * 100m / x.Exam.MaxScore : 0)))
                .ToList();

            var teachingAssignmentIds = await _db.TeachingAssignments.AsNoTracking()
                .Where(ta => ta.ClassRoomId == ce.ClassRoom.Id)
                .Select(ta => ta.Id)
                .ToListAsync(ct);

            var pendingAssignments = 0;
            foreach (var taId in teachingAssignmentIds)
            {
                pendingAssignments += await _db.Assignments.AsNoTracking()
                    .Where(a => a.TeachingAssignmentId == taId)
                    .CountAsync(a => !_db.AssignmentSubmissions
                        .Any(s => s.AssignmentId == a.Id && s.StudentEnrollmentId == enrollmentId), ct);
            }

            var termIds = await _db.TeachingAssignments.AsNoTracking()
                .Where(ta => ta.ClassRoomId == ce.ClassRoom.Id)
                .Select(ta => ta.TermId)
                .Distinct()
                .ToListAsync(ct);

            var upcomingExams = await _db.ExamSchedules.AsNoTracking()
                .Where(es => es.ExamDate > DateTime.UtcNow)
                .Join(_db.Exams, es => es.ExamId, ex => ex.Id, (es, ex) => new { es, Exam = ex })
                .Where(x => termIds.Contains(x.Exam.TermId))
                .CountAsync(ct);

            children.Add(new ParentChildDashboardDto(
                ce.Student.Id,
                ce.Enrollment.Id,
                ce.Student.FirstName,
                ce.Student.LastName,
                ce.ClassRoom.Name,
                attendancePct,
                recentGrades,
                pendingAssignments,
                upcomingExams));
        }

        var unreadCount = await _db.Notifications.AsNoTracking()
            .Where(n => n.RecipientUserId == userId && n.Status == NotificationStatus.Delivered)
            .CountAsync(ct);

        var outstandingInvoices = await _platformDb.Invoices.AsNoTracking()
            .Where(i => i.SchoolId == schoolId &&
                i.Status != Domain.Entities.Billing.InvoiceStatus.Paid &&
                i.Status != Domain.Entities.Billing.InvoiceStatus.Cancelled)
            .ToListAsync(ct);

        return new ParentDashboardDto(
            children,
            unreadCount,
            outstandingInvoices.Count,
            outstandingInvoices.Sum(i => i.BalanceDue));
    }

    public async Task<ParentChildProfileDto?> GetParentChildProfileAsync(
        Guid schoolId, Guid userId, Guid studentId, CancellationToken ct = default)
    {
        var parentId = await _db.Parents.AsNoTracking()
            .Where(p => p.SchoolId == schoolId && p.ApplicationUserId == userId)
            .Select(p => p.Id)
            .FirstOrDefaultAsync(ct);

        if (parentId == Guid.Empty) return null;

        var hasGuardian = await _db.StudentGuardians.AsNoTracking()
            .AnyAsync(g => g.ParentId == parentId && g.StudentId == studentId, ct);

        if (!hasGuardian) return null;

        var student = await _db.Students.AsNoTracking()
            .Where(s => s.Id == studentId && s.SchoolId == schoolId)
            .FirstOrDefaultAsync(ct);

        if (student is null) return null;

        var enrollment = await _db.StudentEnrollments.AsNoTracking()
            .Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Active)
            .Join(_db.ClassRooms, e => e.ClassRoomId, c => c.Id, (e, c) => new { e, ClassRoom = c })
            .Join(_db.GradeLevels, x => x.ClassRoom.GradeLevelId, g => g.Id, (x, g) => new { x.e, x.ClassRoom, GradeLevel = g })
            .Join(_db.AcademicYears, x => x.e.AcademicYearId, a => a.Id, (x, a) => new { x.e, x.ClassRoom, x.GradeLevel, AcademicYear = a })
            .FirstOrDefaultAsync(ct);

        var enrollmentId = enrollment?.e.Id ?? Guid.Empty;

        var attendanceRecords = await _db.AttendanceRecords.AsNoTracking()
            .Where(ar => ar.StudentEnrollmentId == enrollmentId)
            .ToListAsync(ct);

        var totalDays = attendanceRecords.Count;
        var presentDays = attendanceRecords.Count(ar => ar.Status == AttendanceStatus.Present);
        var attendancePct = totalDays > 0 ? Math.Round(presentDays * 100m / totalDays, 1) : 100m;

        var totalAssignments = 0;
        var gradedAssignments = 0;
        if (enrollment != null)
        {
            var teachingAssignmentIds = await _db.TeachingAssignments.AsNoTracking()
                .Where(ta => ta.ClassRoomId == enrollment.ClassRoom.Id)
                .Select(ta => ta.Id)
                .ToListAsync(ct);

            totalAssignments = await _db.Assignments.AsNoTracking()
                .Where(a => teachingAssignmentIds.Contains(a.TeachingAssignmentId))
                .CountAsync(ct);

            gradedAssignments = await _db.AssignmentSubmissions.AsNoTracking()
                .Where(s => s.StudentEnrollmentId == enrollmentId && s.Status == SubmissionStatus.Graded)
                .CountAsync(ct);
        }

        var latestReportCard = await _db.ReportCards.AsNoTracking()
            .Where(rc => rc.StudentEnrollmentId == enrollmentId)
            .OrderByDescending(rc => rc.GeneratedAtUtc)
            .Select(rc => new { rc.OverallPercentage, rc.OverallGrade })
            .FirstOrDefaultAsync(ct);

        return new ParentChildProfileDto(
            student.Id,
            student.StudentCode,
            student.FirstName,
            student.LastName,
            student.DateOfBirth,
            enrollment?.ClassRoom.Name,
            enrollment?.GradeLevel.Name,
            enrollment?.AcademicYear.Name,
            attendancePct,
            totalAssignments,
            gradedAssignments,
            latestReportCard != null ? latestReportCard.OverallPercentage : null,
            latestReportCard != null ? latestReportCard.OverallGrade : null);
    }

    public async Task<PagedResult<PortalAttendanceRecordDto>> GetParentChildAttendanceAsync(
        Guid schoolId, Guid userId, Guid studentId, PaginationParams p, CancellationToken ct = default)
    {
        var parentId = await _db.Parents.AsNoTracking()
            .Where(pp => pp.SchoolId == schoolId && pp.ApplicationUserId == userId)
            .Select(pp => pp.Id)
            .FirstOrDefaultAsync(ct);

        if (parentId == Guid.Empty)
            return PagedResult<PortalAttendanceRecordDto>.Empty(p.Page, p.PageSize);

        var hasGuardian = await _db.StudentGuardians.AsNoTracking()
            .AnyAsync(g => g.ParentId == parentId && g.StudentId == studentId, ct);

        if (!hasGuardian)
            return PagedResult<PortalAttendanceRecordDto>.Empty(p.Page, p.PageSize);

        var enrollmentIds = await _db.StudentEnrollments.AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .Select(e => e.Id)
            .ToListAsync(ct);

        var items = await _db.AttendanceRecords.AsNoTracking()
            .Where(ar => enrollmentIds.Contains(ar.StudentEnrollmentId))
            .Join(_db.AttendanceSessions,
                ar => ar.AttendanceSessionId, asess => asess.Id,
                (ar, asess) => new { ar, Session = asess })
            .Join(_db.ClassSchedules,
                x => x.Session.ClassScheduleId, cs => cs.Id,
                (x, cs) => new { x.ar, x.Session, cs })
            .Join(_db.TeachingAssignments,
                x => x.cs.TeachingAssignmentId, ta => ta.Id,
                (x, ta) => new { x.ar, x.Session, x.cs, ta })
            .Join(_db.Subjects,
                x => x.ta.SubjectId, s => s.Id,
                (x, s) => new PortalAttendanceRecordDto(
                    x.Session.Date,
                    s.Name,
                    x.ar.Status.ToString(),
                    x.ar.Note))
            .OrderByDescending(x => x.Date)
            .Skip(p.Skip)
            .Take(p.PageSize)
            .ToListAsync(ct);

        var total = await _db.AttendanceRecords.AsNoTracking()
            .Where(ar => enrollmentIds.Contains(ar.StudentEnrollmentId))
            .CountAsync(ct);

        return new PagedResult<PortalAttendanceRecordDto>(items, total, p.Page, p.PageSize);
    }

    public async Task<PagedResult<PortalExamResultDto>> GetParentChildGradesAsync(
        Guid schoolId, Guid userId, Guid studentId, Guid? termId, PaginationParams p, CancellationToken ct = default)
    {
        var parentId = await _db.Parents.AsNoTracking()
            .Where(pp => pp.SchoolId == schoolId && pp.ApplicationUserId == userId)
            .Select(pp => pp.Id)
            .FirstOrDefaultAsync(ct);

        if (parentId == Guid.Empty)
            return PagedResult<PortalExamResultDto>.Empty(p.Page, p.PageSize);

        var hasGuardian = await _db.StudentGuardians.AsNoTracking()
            .AnyAsync(g => g.ParentId == parentId && g.StudentId == studentId, ct);

        if (!hasGuardian)
            return PagedResult<PortalExamResultDto>.Empty(p.Page, p.PageSize);

        var enrollmentIds = await _db.StudentEnrollments.AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .Select(e => e.Id)
            .ToListAsync(ct);

        var baseQuery = _db.ExamResults.AsNoTracking()
            .Where(er => enrollmentIds.Contains(er.StudentEnrollmentId))
            .Join(_db.Exams, er => er.ExamId, ex => ex.Id, (er, ex) => new { er, Exam = ex })
            .Join(_db.Subjects, x => x.Exam.SubjectId, s => s.Id, (x, s) => new { x.er, x.Exam, Subject = s })
            .AsQueryable();

        if (termId.HasValue)
            baseQuery = baseQuery.Where(x => x.Exam.TermId == termId.Value);

        var allData = await baseQuery
            .OrderByDescending(x => x.er.Score)
            .Skip(p.Skip)
            .Take(p.PageSize)
            .ToListAsync(ct);

        var total = await baseQuery.CountAsync(ct);

        var items = allData
            .Select(x => new PortalExamResultDto(
                x.Subject.Name,
                x.Exam.Name,
                x.er.Score,
                x.Exam.MaxScore,
                x.Exam.MaxScore > 0 ? Math.Round(x.er.Score * 100m / x.Exam.MaxScore, 1) : 0,
                CalculateGrade(x.Exam.MaxScore > 0 ? x.er.Score * 100m / x.Exam.MaxScore : 0),
                x.er.Remarks))
            .ToList();

        return new PagedResult<PortalExamResultDto>(items, total, p.Page, p.PageSize);
    }

    public async Task<PagedResult<PortalAssignmentDto>> GetParentChildAssignmentsAsync(
        Guid schoolId, Guid userId, Guid studentId, PaginationParams p, CancellationToken ct = default)
    {
        var parentId = await _db.Parents.AsNoTracking()
            .Where(pp => pp.SchoolId == schoolId && pp.ApplicationUserId == userId)
            .Select(pp => pp.Id)
            .FirstOrDefaultAsync(ct);

        if (parentId == Guid.Empty)
            return PagedResult<PortalAssignmentDto>.Empty(p.Page, p.PageSize);

        var hasGuardian = await _db.StudentGuardians.AsNoTracking()
            .AnyAsync(g => g.ParentId == parentId && g.StudentId == studentId, ct);

        if (!hasGuardian)
            return PagedResult<PortalAssignmentDto>.Empty(p.Page, p.PageSize);

        var enrollmentIds = await _db.StudentEnrollments.AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .Select(e => e.Id)
            .ToListAsync(ct);

        var allData = await _db.Assignments.AsNoTracking()
            .Join(_db.TeachingAssignments,
                a => a.TeachingAssignmentId, ta => ta.Id,
                (a, ta) => new { a, ta })
            .Join(_db.Subjects,
                x => x.ta.SubjectId, s => s.Id,
                (x, s) => new { x.a, x.ta, Subject = s })
            .Join(_db.Teachers,
                x => x.ta.TeacherId, t => t.Id,
                (x, t) => new { x.a, x.ta, x.Subject, Teacher = t })
            .OrderByDescending(x => x.a.DueDate)
            .Skip(p.Skip)
            .Take(p.PageSize)
            .ToListAsync(ct);

        var total = await _db.Assignments.AsNoTracking()
            .Join(_db.TeachingAssignments,
                a => a.TeachingAssignmentId, ta => ta.Id,
                (a, ta) => new { a, ta })
            .CountAsync(ct);

        var items = new List<PortalAssignmentDto>();
        foreach (var x in allData)
        {
            var submission = await _db.AssignmentSubmissions.AsNoTracking()
                .FirstOrDefaultAsync(s => s.AssignmentId == x.a.Id && enrollmentIds.Contains(s.StudentEnrollmentId), ct);

            items.Add(new PortalAssignmentDto(
                x.a.Id,
                x.a.Title,
                x.Subject.Name,
                $"{x.Teacher.FirstName} {x.Teacher.LastName}",
                x.a.DueDate,
                x.a.MaxScore,
                submission?.Status,
                submission?.Grade,
                submission?.TeacherFeedback,
                submission?.SubmittedAtUtc));
        }

        return new PagedResult<PortalAssignmentDto>(items, total, p.Page, p.PageSize);
    }

    public async Task<PagedResult<PortalReportCardDto>> GetParentChildReportCardsAsync(
        Guid schoolId, Guid userId, Guid studentId, PaginationParams p, CancellationToken ct = default)
    {
        var parentId = await _db.Parents.AsNoTracking()
            .Where(pp => pp.SchoolId == schoolId && pp.ApplicationUserId == userId)
            .Select(pp => pp.Id)
            .FirstOrDefaultAsync(ct);

        if (parentId == Guid.Empty)
            return PagedResult<PortalReportCardDto>.Empty(p.Page, p.PageSize);

        var hasGuardian = await _db.StudentGuardians.AsNoTracking()
            .AnyAsync(g => g.ParentId == parentId && g.StudentId == studentId, ct);

        if (!hasGuardian)
            return PagedResult<PortalReportCardDto>.Empty(p.Page, p.PageSize);

        var enrollmentIds = await _db.StudentEnrollments.AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .Select(e => e.Id)
            .ToListAsync(ct);

        var query = _db.ReportCards.AsNoTracking()
            .Where(rc => enrollmentIds.Contains(rc.StudentEnrollmentId))
            .Join(_db.Terms, rc => rc.TermId, t => t.Id, (rc, t) => new { rc, Term = t });

        var total = await query.CountAsync(ct);

        var allData = await query
            .OrderByDescending(x => x.rc.GeneratedAtUtc)
            .Skip(p.Skip)
            .Take(p.PageSize)
            .ToListAsync(ct);

        var items = new List<PortalReportCardDto>();
        foreach (var x in allData)
        {
            var subjects = await _db.ReportCardSubjectResults.AsNoTracking()
                .Where(s => s.ReportCardId == x.rc.Id)
                .Select(s => new PortalReportCardSubjectDto(
                    s.SubjectName, s.Score, s.MaxScore, s.Grade))
                .ToListAsync(ct);

            items.Add(new PortalReportCardDto(
                x.rc.Id,
                x.Term.Name,
                x.rc.OverallPercentage,
                x.rc.OverallGrade,
                x.rc.IsLocked,
                x.rc.GeneratedAtUtc,
                subjects));
        }

        return new PagedResult<PortalReportCardDto>(items, total, p.Page, p.PageSize);
    }

    public async Task<PagedResult<PortalInvoiceDto>> GetParentBillingAsync(
        Guid schoolId, PaginationParams p, CancellationToken ct = default)
    {
        var total = await _platformDb.Invoices.AsNoTracking()
            .CountAsync(i => i.SchoolId == schoolId, ct);

        var invoiceData = await _platformDb.Invoices.AsNoTracking()
            .Where(i => i.SchoolId == schoolId)
            .OrderByDescending(i => i.DueDate)
            .Skip(p.Skip)
            .Take(p.PageSize)
            .ToListAsync(ct);

        var items = new List<PortalInvoiceDto>();
        foreach (var i in invoiceData)
        {
            var paidAmount = await _platformDb.Payments.AsNoTracking()
                .Where(pay => pay.InvoiceId == i.Id)
                .SumAsync(pay => pay.Amount, ct);

            items.Add(new PortalInvoiceDto(
                i.Id,
                i.Amount,
                paidAmount,
                i.Amount - paidAmount,
                i.DueDate,
                i.Status.ToString(),
                i.CreatedAtUtc));
        }

        return new PagedResult<PortalInvoiceDto>(items, total, p.Page, p.PageSize);
    }

    public async Task<List<PortalNotificationDto>> GetParentNotificationsAsync(
        Guid userId, int limit, CancellationToken ct = default)
    {
        return await _db.Notifications.AsNoTracking()
            .Where(n => n.RecipientUserId == userId)
            .Join(_db.NotificationBatches,
                n => n.NotificationBatchId, b => b.Id,
                (n, b) => new { n, Batch = b })
            .OrderByDescending(x => x.Batch.CreatedAtUtc)
            .Take(limit)
            .Select(x => new PortalNotificationDto(
                x.n.Id,
                x.Batch.Subject,
                x.Batch.Body,
                x.n.Status.ToString(),
                x.Batch.CreatedAtUtc,
                x.n.ReadAtUtc))
            .ToListAsync(ct);
    }

    // ════════════════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════════════════

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
