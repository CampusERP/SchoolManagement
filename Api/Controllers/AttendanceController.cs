using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Common.Models;
using Application.Features.Attendance.Commands.RecordAttendance;
using Application.Features.Attendance.Commands.LockAttendanceSession;
using Application.Features.Attendance.Queries.GetClassAttendance;
using Domain.Enums;

namespace Api.Controllers;

[Authorize]
public class AttendanceController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public AttendanceController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Record attendance for an entire class in one request.
    /// Accepts an array of { studentEnrollmentId, status, note? }.
    /// Idempotent — calling again for the same session updates existing records.
    /// Teacher only.
    /// </summary>
    [HttpPost("sessions")]
    [Authorize(Policy = "Attendance.Record")]
    public async Task<IActionResult> RecordAttendance(
        [FromBody] RecordAttendanceCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));

    /// <summary>
    /// Get the attendance sheet for a specific class-day (teacher view).
    /// Returns all student statuses for that session.
    /// </summary>
    [HttpGet("sessions")]
    [Authorize(Policy = "Attendance.Record")]
    public async Task<IActionResult> GetClassAttendance(
        [FromQuery] Guid schoolId,
        [FromQuery] Guid classScheduleId,
        [FromQuery] DateOnly date,
        CancellationToken ct) =>
        FromResult(await _mediator.Send(
            new GetClassAttendanceQuery(schoolId, classScheduleId, date), ct));

    /// <summary>
    /// Lock an attendance session to prevent further modifications.
    /// Teacher only. Session must have at least one record.
    /// </summary>
    [HttpPost("sessions/{attendanceSessionId:guid}/lock")]
    [Authorize(Policy = "Attendance.Record")]
    public async Task<IActionResult> LockAttendanceSession(
        Guid attendanceSessionId,
        [FromQuery] Guid schoolId,
        CancellationToken ct) =>
        FromResult(await _mediator.Send(
            new LockAttendanceSessionCommand(schoolId, attendanceSessionId), ct));

    /// <summary>
    /// Get paginated attendance history for a single student.
    /// Supports date range, status, and academic year filters.
    /// Used by student portal and parent portal.
    /// </summary>
    [HttpGet("students/{enrollmentId:guid}")]
    [Authorize(Policy = "Attendance.ReadOwn")]
    public async Task<IActionResult> GetStudentAttendance(
        Guid enrollmentId,
        [FromQuery] Guid schoolId,
        [FromQuery] Guid? academicYearId,
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] AttendanceStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        FromResult(await _mediator.Send(
            new GetStudentAttendanceQuery(schoolId, enrollmentId, academicYearId,
                dateFrom, dateTo, status, new PaginationParams(page, pageSize)), ct));

    /// <summary>
    /// Get attendance summary with statistics for a student.
    /// Returns total/present/absent/late days and percentage.
    /// </summary>
    [HttpGet("students/{enrollmentId:guid}/summary")]
    [Authorize(Policy = "Attendance.ReadOwn")]
    public async Task<IActionResult> GetStudentAttendanceSummary(
        Guid enrollmentId,
        [FromQuery] Guid schoolId,
        CancellationToken ct) =>
        FromResult(await _mediator.Send(
            new GetStudentAttendanceSummaryQuery(schoolId, enrollmentId), ct));
}
