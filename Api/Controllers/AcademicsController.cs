using Application.Features.Academics.CreateAcademicYear;
using Application.Features.Academics.CreateClassRoom;
using Application.Features.Academics.CreateGradeLevel;
using Application.Features.Academics.CreateRoom;
using Application.Features.Academics.CreateTerm;
using Application.Features.Academics.UpdateAcademicYear;
using Application.Features.Academics.UpdateClassRoom;
using Application.Features.Academics.UpdateGradeLevel;
using Application.Features.Academics.UpdateRoom;
using Application.Features.Academics.Queries.GetAcademicYears;
using Application.Features.Academics.Queries.GetClassRooms;
using Application.Features.Academics.Queries.GetGradeLevels;
using Application.Features.Academics.Queries.GetRooms;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
public class AcademicsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public AcademicsController(IMediator mediator) => _mediator = mediator;

    // ── Academic Years ────────────────────────────────────────────────

    [HttpGet("academic-years")]
    [Authorize(Policy = "AcademicYear.Read")]
    public async Task<IActionResult> GetAcademicYears(
        [FromQuery] Guid schoolId, CancellationToken ct) =>
        FromResult(await _mediator.Send(new GetAcademicYearsQuery(schoolId), ct));

    [HttpPost("academic-years")]
    [Authorize(Policy = "AcademicYear.Create")]
    public async Task<IActionResult> CreateAcademicYear(
        [FromBody] CreateAcademicYearCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));

    [HttpPut("academic-years/{academicYearId:guid}")]
    [Authorize(Policy = "AcademicYear.Update")]
    public async Task<IActionResult> UpdateAcademicYear(
        Guid academicYearId,
        [FromBody] UpdateAcademicYearCommand command, CancellationToken ct) =>
        FromResult(await _mediator.Send(command with { AcademicYearId = academicYearId }, ct));

    [HttpPost("academic-years/{academicYearId:guid}/terms")]
    [Authorize(Policy = "AcademicYear.Create")]
    public async Task<IActionResult> CreateTerm(
        Guid academicYearId,
        [FromBody] CreateTermCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command with { AcademicYearId = academicYearId }, ct));

    // ── Classrooms ────────────────────────────────────────────────────

    [HttpGet("classrooms")]
    [Authorize(Policy = "ClassRoom.Read")]
    public async Task<IActionResult> GetClassRooms(
        [FromQuery] Guid schoolId,
        [FromQuery] Guid? academicYearId,
        [FromQuery] Guid? gradeLevelId,
        CancellationToken ct) =>
        FromResult(await _mediator.Send(new GetClassRoomsQuery(schoolId, academicYearId, gradeLevelId), ct));

    [HttpPost("classrooms")]
    [Authorize(Policy = "ClassRoom.Create")]
    public async Task<IActionResult> CreateClassRoom(
        [FromBody] CreateClassRoomCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));

    [HttpPut("classrooms/{classRoomId:guid}")]
    [Authorize(Policy = "ClassRoom.Update")]
    public async Task<IActionResult> UpdateClassRoom(
        Guid classRoomId,
        [FromBody] UpdateClassRoomCommand command, CancellationToken ct) =>
        FromResult(await _mediator.Send(command with { ClassRoomId = classRoomId }, ct));

    // ── Grade Levels ──────────────────────────────────────────────────

    [HttpGet("grade-levels")]
    [Authorize(Policy = "GradeLevel.Read")]
    public async Task<IActionResult> GetGradeLevels(
        [FromQuery] Guid schoolId, CancellationToken ct) =>
        FromResult(await _mediator.Send(new GetGradeLevelsQuery(schoolId), ct));

    [HttpPost("grade-levels")]
    [Authorize(Policy = "AcademicYear.Create")]
    public async Task<IActionResult> CreateGradeLevel(
        [FromBody] CreateGradeLevelCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));

    [HttpPut("grade-levels/{gradeLevelId:guid}")]
    [Authorize(Policy = "GradeLevel.Update")]
    public async Task<IActionResult> UpdateGradeLevel(
        Guid gradeLevelId,
        [FromBody] UpdateGradeLevelCommand command, CancellationToken ct) =>
        FromResult(await _mediator.Send(command with { GradeLevelId = gradeLevelId }, ct));

    // ── Rooms ─────────────────────────────────────────────────────────

    [HttpGet("rooms")]
    [Authorize(Policy = "Room.Read")]
    public async Task<IActionResult> GetRooms(
        [FromQuery] Guid schoolId, CancellationToken ct) =>
        FromResult(await _mediator.Send(new GetRoomsQuery(schoolId), ct));

    [HttpPost("rooms")]
    [Authorize(Policy = "ClassRoom.Create")]
    public async Task<IActionResult> CreateRoom(
        [FromBody] CreateRoomCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));

    [HttpPut("rooms/{roomId:guid}")]
    [Authorize(Policy = "Room.Update")]
    public async Task<IActionResult> UpdateRoom(
        Guid roomId,
        [FromBody] UpdateRoomCommand command, CancellationToken ct) =>
        FromResult(await _mediator.Send(command with { RoomId = roomId }, ct));
}