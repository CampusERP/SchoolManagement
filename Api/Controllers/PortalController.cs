using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Common.Models;
using Application.Features.Portal.Queries.GetTeacherSchedule;

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

    // ── TEACHER PORTAL ────────────────────────────────────────────────

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

    // ── STUDENT PORTAL ────────────────────────────────────────────────

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

    // ── PARENT PORTAL ─────────────────────────────────────────────────

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
}
