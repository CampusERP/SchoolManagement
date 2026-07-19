using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Common.Models;
using Application.Features.Attendance.Commands.RecordAttendance;
using Application.Features.Attendance.Queries.GetClassAttendance;

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
    /// Get paginated attendance history for a single student.
    /// Used by student portal and parent portal.
    /// </summary>
    [HttpGet("students/{enrollmentId:guid}")]
    [Authorize(Policy = "Attendance.ReadOwn")]
    public async Task<IActionResult> GetStudentAttendance(
        Guid enrollmentId,
        [FromQuery] Guid schoolId,
        [FromQuery] Guid? academicYearId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        FromResult(result: await _mediator.Send(
            new GetStudentAttendanceQuery(schoolId, enrollmentId, academicYearId,
                new PaginationParams(page, pageSize)), ct));
}
