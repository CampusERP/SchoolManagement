using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Common.Models;
using Application.Features.Portal.Queries.GetTeacherDashboard;
using Application.Features.Portal.Queries.GetTeacherSchedule;
using Application.Features.Portal.Queries.GetStudentDashboard;
using Application.Features.Portal.Queries.GetStudentClasses;
using Application.Features.Portal.Queries.GetStudentNotifications;
using Application.Features.Portal.Queries.GetParentDashboard;
using Application.Features.Portal.Queries.GetParentChildProfile;
using Application.Features.Portal.Queries.GetParentChildAttendance;
using Application.Features.Portal.Queries.GetParentChildGrades;
using Application.Features.Portal.Queries.GetParentChildAssignments;
using Application.Features.Portal.Queries.GetParentChildReportCards;
using Application.Features.Portal.Queries.GetParentBilling;
using Application.Features.Portal.Queries.GetParentNotifications;
using Application.Features.Assignments.Queries.GetClassAssignments;
using Application.Features.Attendance.Queries.GetClassAttendance;
using Application.Features.Exams.Queries;
using Application.Features.Notifications.Queries;
using Application.Features.People.Queries.StudentDetails;

namespace Api.Controllers;

/// <summary>
/// Read-only projection endpoints for the three portals.
/// Every handler here is a pure query: AsNoTracking, no aggregate loading,
/// direct projection to DTOs. These never write to the database.
/// Paginated on every list endpoint — never return unbounded lists.
/// </summary>
[Authorize]
public class PortalController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public PortalController(IMediator mediator) => _mediator = mediator;

    // ════════════════════════════════════════════════════════
    //  TEACHER PORTAL
    // ════════════════════════════════════════════════════════

    /// <summary>
    /// Teacher's dashboard: classes, today's schedule, pending attendance, pending assignments, announcements.
    /// </summary>
    [HttpGet("teacher/dashboard")]
    [Authorize(Policy = "Schedule.Read")]
    public async Task<IActionResult> GetTeacherDashboard(
        [FromQuery] Guid schoolId,
        CancellationToken ct) =>
        FromResult(await _mediator.Send(
            new GetTeacherDashboardQuery(schoolId), ct));

    /// <summary>
    /// Teacher's weekly timetable for a given term.
    /// Returns all ClassSchedule slots with subject, room, and class info.
    /// </summary>
    [HttpGet("teacher/{teacherId:guid}/schedule")]
    [Authorize(Policy = "Schedule.Read")]
    public async Task<IActionResult> GetTeacherSchedule(
        Guid teacherId,
        [FromQuery] Guid schoolId,
        [FromQuery] Guid termId,
        CancellationToken ct) =>
        FromResult(await _mediator.Send(
            new GetTeacherScheduleQuery(schoolId, teacherId, termId), ct));

    /// <summary>
    /// Roster (student list) for a classroom.
    /// Paginated. Used by teachers for attendance, grading, and class management.
    /// </summary>
    [HttpGet("classrooms/{classRoomId:guid}/roster")]
    [Authorize(Policy = "Schedule.Read")]
    public async Task<IActionResult> GetClassRoomRoster(
        Guid classRoomId,
        [FromQuery] Guid schoolId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default) =>
        FromResult(await _mediator.Send(
            new GetClassRoomRosterQuery(schoolId, classRoomId,
                new PaginationParams(page, pageSize)), ct));

    // ════════════════════════════════════════════════════════
    //  STUDENT PORTAL
    // ════════════════════════════════════════════════════════

    /// <summary>
    /// Student's full dashboard: profile, schedule, assignments, attendance, grades, notifications.
    /// </summary>
    [HttpGet("student/{enrollmentId:guid}/dashboard")]
    [Authorize(Policy = "Profile.Read")]
    public async Task<IActionResult> GetStudentDashboard(
        Guid enrollmentId,
        [FromQuery] Guid schoolId,
        CancellationToken ct) =>
        FromResult(await _mediator.Send(
            new GetStudentDashboardQuery(schoolId, enrollmentId), ct));

    /// <summary>
    /// Student's profile details.
    /// </summary>
    [HttpGet("student/profile")]
    [Authorize(Policy = "Profile.Read")]
    public async Task<IActionResult> GetStudentProfile(
        [FromQuery] Guid schoolId,
        CancellationToken ct) =>
        FromResult(await _mediator.Send(
            new GetMyProfileQuery(schoolId), ct));

    /// <summary>
    /// Student's weekly timetable for a given term.
    /// Returns subject, teacher name, room, and time for each slot.
    /// </summary>
    [HttpGet("student/{enrollmentId:guid}/schedule")]
    [Authorize(Policy = "Attendance.ReadOwn")]
    public async Task<IActionResult> GetStudentSchedule(
        Guid enrollmentId,
        [FromQuery] Guid schoolId,
        [FromQuery] Guid termId,
        CancellationToken ct) =>
        FromResult(await _mediator.Send(
            new GetStudentScheduleQuery(schoolId, enrollmentId, termId), ct));

    /// <summary>
    /// Student's current classes with subject info and schedule.
    /// </summary>
    [HttpGet("student/{enrollmentId:guid}/classes")]
    [Authorize(Policy = "MyClasses.Read")]
    public async Task<IActionResult> GetStudentClasses(
        Guid enrollmentId,
        [FromQuery] Guid schoolId,
        CancellationToken ct) =>
        FromResult(await _mediator.Send(
            new GetStudentClassesQuery(schoolId, enrollmentId), ct));

    /// <summary>
    /// Student's assignments across all classes.
    /// </summary>
    [HttpGet("student/{enrollmentId:guid}/assignments")]
    [Authorize(Policy = "Assignment.ReadOwn")]
    public async Task<IActionResult> GetStudentAssignments(
        Guid enrollmentId,
        [FromQuery] Guid schoolId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        FromResult(await _mediator.Send(
            new GetStudentAssignmentsQuery(schoolId, enrollmentId,
                new PaginationParams(page, pageSize)), ct));

    /// <summary>
    /// Student's attendance history.
    /// </summary>
    [HttpGet("student/{enrollmentId:guid}/attendance")]
    [Authorize(Policy = "Attendance.ReadOwn")]
    public async Task<IActionResult> GetStudentAttendance(
        Guid enrollmentId,
        [FromQuery] Guid schoolId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        FromResult(await _mediator.Send(
            new GetStudentAttendanceQuery(schoolId, enrollmentId,
                Pagination: new PaginationParams(page, pageSize)), ct));

    /// <summary>
    /// Student's exam results.
    /// </summary>
    [HttpGet("student/{enrollmentId:guid}/exams")]
    [Authorize(Policy = "Exam.Read")]
    public async Task<IActionResult> GetStudentExams(
        Guid enrollmentId,
        [FromQuery] Guid schoolId,
        [FromQuery] Guid? termId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        FromResult(await _mediator.Send(
            new GetStudentExamResultsQuery(schoolId, enrollmentId, termId,
                new PaginationParams(page, pageSize)), ct));

    /// <summary>
    /// Student's report cards.
    /// </summary>
    [HttpGet("student/{enrollmentId:guid}/report-cards")]
    [Authorize(Policy = "Exam.Read")]
    public async Task<IActionResult> GetStudentReportCards(
        Guid enrollmentId,
        [FromQuery] Guid schoolId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default) =>
        FromResult(await _mediator.Send(
            new GetReportCardsQuery(schoolId, enrollmentId,
                new PaginationParams(page, pageSize)), ct));

    /// <summary>
    /// Student's notifications.
    /// </summary>
    [HttpGet("student/notifications")]
    [Authorize(Policy = "Notification.Read")]
    public async Task<IActionResult> GetStudentNotifications(
        [FromQuery] Guid schoolId,
        [FromQuery] int limit = 20,
        CancellationToken ct = default) =>
        FromResult(await _mediator.Send(
            new GetStudentNotificationsQuery(schoolId, limit), ct));

    /// <summary>
    /// At-a-glance summary for one child: class, attendance %, pending assignments.
    /// Used by parent portal home screen. Single fast query — no graph loading.
    /// </summary>
    [HttpGet("student/{enrollmentId:guid}/summary")]
    [Authorize(Policy = "Attendance.ReadChild")]
    public async Task<IActionResult> GetStudentSummary(
        Guid enrollmentId,
        [FromQuery] Guid schoolId,
        CancellationToken ct) =>
        FromResult(await _mediator.Send(
            new GetStudentSummaryQuery(schoolId, enrollmentId), ct));

    // ════════════════════════════════════════════════════════
    //  PARENT PORTAL
    // ════════════════════════════════════════════════════════

    /// <summary>
    /// Parent's full dashboard: children overview, attendance, grades, invoices, notifications.
    /// </summary>
    [HttpGet("parent/dashboard")]
    [Authorize(Policy = "Children.Read")]
    public async Task<IActionResult> GetParentDashboard(
        [FromQuery] Guid schoolId,
        CancellationToken ct) =>
        FromResult(await _mediator.Send(
            new GetParentDashboardQuery(schoolId), ct));

    /// <summary>
    /// Detailed profile of a child: academic info, attendance, assignments, grades.
    /// Validates parent has a guardian relationship with the student.
    /// </summary>
    [HttpGet("parent/children/{studentId:guid}/profile")]
    [Authorize(Policy = "Children.Read")]
    public async Task<IActionResult> GetParentChildProfile(
        Guid studentId,
        [FromQuery] Guid schoolId,
        CancellationToken ct) =>
        FromResult(await _mediator.Send(
            new GetParentChildProfileQuery(schoolId, studentId), ct));

    /// <summary>
    /// Parent viewing a child's attendance history.
    /// </summary>
    [HttpGet("parent/children/{studentId:guid}/attendance")]
    [Authorize(Policy = "Attendance.ReadChild")]
    public async Task<IActionResult> GetParentChildAttendance(
        Guid studentId,
        [FromQuery] Guid schoolId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        FromResult(await _mediator.Send(
            new GetParentChildAttendanceQuery(schoolId, studentId,
                new PaginationParams(page, pageSize)), ct));

    /// <summary>
    /// Parent viewing a child's exam grades.
    /// </summary>
    [HttpGet("parent/children/{studentId:guid}/grades")]
    [Authorize(Policy = "Children.Read")]
    public async Task<IActionResult> GetParentChildGrades(
        Guid studentId,
        [FromQuery] Guid schoolId,
        [FromQuery] Guid? termId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        FromResult(await _mediator.Send(
            new GetParentChildGradesQuery(schoolId, studentId, termId,
                new PaginationParams(page, pageSize)), ct));

    /// <summary>
    /// Parent viewing a child's assignments.
    /// </summary>
    [HttpGet("parent/children/{studentId:guid}/assignments")]
    [Authorize(Policy = "Children.Read")]
    public async Task<IActionResult> GetParentChildAssignments(
        Guid studentId,
        [FromQuery] Guid schoolId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        FromResult(await _mediator.Send(
            new GetParentChildAssignmentsQuery(schoolId, studentId,
                new PaginationParams(page, pageSize)), ct));

    /// <summary>
    /// Parent viewing a child's report cards.
    /// </summary>
    [HttpGet("parent/children/{studentId:guid}/report-cards")]
    [Authorize(Policy = "Children.Read")]
    public async Task<IActionResult> GetParentChildReportCards(
        Guid studentId,
        [FromQuery] Guid schoolId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default) =>
        FromResult(await _mediator.Send(
            new GetParentChildReportCardsQuery(schoolId, studentId,
                new PaginationParams(page, pageSize)), ct));

    /// <summary>
    /// Parent's billing: invoices for the school.
    /// </summary>
    [HttpGet("parent/billing")]
    [Authorize(Policy = "Children.Read")]
    public async Task<IActionResult> GetParentBilling(
        [FromQuery] Guid schoolId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        FromResult(await _mediator.Send(
            new GetParentBillingQuery(schoolId,
                new PaginationParams(page, pageSize)), ct));

    /// <summary>
    /// Parent's notifications.
    /// </summary>
    [HttpGet("parent/notifications")]
    [Authorize(Policy = "Notification.Read")]
    public async Task<IActionResult> GetParentNotifications(
        [FromQuery] Guid schoolId,
        [FromQuery] int limit = 20,
        CancellationToken ct = default) =>
        FromResult(await _mediator.Send(
            new GetParentNotificationsQuery(schoolId, limit), ct));
}
