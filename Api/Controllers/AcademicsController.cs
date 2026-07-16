using Application.Features.Academics.CreateAcademicYear;
using Application.Features.Academics.CreateClassRoom;
using Application.Features.Academics.CreateGradeLevel;
using Application.Features.Academics.CreateRoom;
using Application.Features.Academics.CreateTerm;
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

    [HttpPost("academic-years")]
    [Authorize(Policy = "AcademicYear.Create")]
    public async Task<IActionResult> CreateAcademicYear(
        [FromBody] CreateAcademicYearCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));

    [HttpPost("academic-years/{academicYearId:guid}/terms")]
    [Authorize(Policy = "AcademicYear.Create")]
    public async Task<IActionResult> CreateTerm(
    Guid academicYearId,
    [FromBody] CreateTermCommand command, CancellationToken ct) =>
    Created(await _mediator.Send(command with { AcademicYearId = academicYearId }, ct));

    // ── Classrooms ────────────────────────────────────────────────────

    [HttpPost("classrooms")]
    [Authorize(Policy = "ClassRoom.Create")]
    public async Task<IActionResult> CreateClassRoom(
        [FromBody] CreateClassRoomCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));

    // ── Grade Levels ──────────────────────────────────────────────────

    [HttpPost("grade-levels")]
    [Authorize(Policy = "AcademicYear.Create")]
    public async Task<IActionResult> CreateGradeLevel(
        [FromBody] CreateGradeLevelCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));

    // ── Rooms ─────────────────────────────────────────────────────────

    [HttpPost("rooms")]
    [Authorize(Policy = "ClassRoom.Create")]
    public async Task<IActionResult> CreateRoom(
        [FromBody] CreateRoomCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));
}