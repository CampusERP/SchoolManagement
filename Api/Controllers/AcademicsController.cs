using Application.Features.Academics.Queries.GetAcademicYears;
using Application.Features.Academics.Queries.GetClassRooms;
using Application.Features.Academics.Queries.GetGradeLevels;
using Application.Features.Academics.Queries.GetRooms;
using Application.Features.Academics.Queries.GetTerms;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Academics.Commands.CreateClassRoom;
using Application.Features.Academics.Commands.CreateRoom;
using Application.Features.Academics.Commands.CreateTerm;
using Application.Features.Academics.Commands.CreateGradeLevel;
using Application.Features.Academics.Commands.CreateAcademicYear;
using Application.Features.Academics.Commands.UpdateClassRoom;
using Application.Features.Academics.Commands.UpdateRoom;
using Application.Features.Academics.Commands.UpdateGradeLevel;
using Application.Features.Academics.Commands.UpdateAcademicYear;
using Application.Features.Academics.Commands.UpdateTerm;
using Application.Features.Academics.Commands.DeleteTerm;
using Application.Features.Academics.Commands.CloseAcademicYear;

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

    [HttpPost("academic-years/{academicYearId:guid}/close")]
    [Authorize(Policy = "AcademicYear.Update")]
    public async Task<IActionResult> CloseAcademicYear(
        Guid academicYearId, [FromQuery] Guid schoolId, CancellationToken ct) =>
        FromResult(await _mediator.Send(new CloseAcademicYearCommand(schoolId, academicYearId), ct));

    [HttpPost("academic-years/{academicYearId:guid}/terms")]
    [Authorize(Policy = "AcademicYear.Create")]
    public async Task<IActionResult> CreateTerm(
        Guid academicYearId,
        [FromBody] CreateTermCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command with { AcademicYearId = academicYearId }, ct));

    [HttpGet("academic-years/{academicYearId:guid}/terms")]
    [Authorize(Policy = "AcademicYear.Read")]
    public async Task<IActionResult> GetTerms(
        Guid academicYearId,
        [FromQuery] Guid schoolId,
        CancellationToken ct) =>
        FromResult(await _mediator.Send(new GetTermsQuery(schoolId, academicYearId), ct));

    [HttpPut("academic-years/{academicYearId:guid}/terms/{termId:guid}")]
    [Authorize(Policy = "AcademicYear.Update")]
    public async Task<IActionResult> UpdateTerm(
        Guid academicYearId,
        Guid termId,
        [FromBody] UpdateTermCommand command,
        CancellationToken ct) =>
        FromResult(await _mediator.Send(
            command with { AcademicYearId = academicYearId, TermId = termId }, ct));

    [HttpDelete("academic-years/{academicYearId:guid}/terms/{termId:guid}")]
    [Authorize(Policy = "AcademicYear.Update")]
    public async Task<IActionResult> DeleteTerm(
        Guid academicYearId,
        Guid termId,
        [FromQuery] Guid schoolId,
        CancellationToken ct) =>
        FromResult(await _mediator.Send(
            new DeleteTermCommand(schoolId, academicYearId, termId), ct));

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
    [Authorize(Policy = "GradeLevel.Create")]
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
    [Authorize(Policy = "Room.Create")]
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
