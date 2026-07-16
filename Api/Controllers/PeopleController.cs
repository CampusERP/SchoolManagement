using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Features.People.Commands.CreateStudent;
using Application.Features.People.Commands.CreateTeacher;
using Application.Features.People.Commands.CreateParent;
using Application.Features.People.Commands.LinkStudentGuardian;
using Microsoft.AspNetCore.Authorization;
using Application.Features.People.Queries.GetStudents;

namespace Api.Controllers;

[Authorize]
public class PeopleController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public PeopleController(IMediator mediator) => _mediator = mediator;

    // ── Students ──────────────────────────────────────────────────────

    [HttpGet("students")]
    [Authorize(Policy = "Student.Read")]
    public async Task<IActionResult> GetStudents(
        [FromQuery] GetStudentsQuery query, CancellationToken ct) =>
        Ok(await _mediator.Send(query, ct));

    [HttpPost("students")]
    [Authorize(Policy = "Student.Create")]
    public async Task<IActionResult> CreateStudent(
        [FromBody] CreateStudentCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));

    // ── Teachers ──────────────────────────────────────────────────────

    [HttpPost("teachers")]
    [Authorize(Policy = "Teacher.Create")]
    public async Task<IActionResult> CreateTeacher(
        [FromBody] CreateTeacherCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));

    // ── Parents ───────────────────────────────────────────────────────

    [HttpPost("parents")]
    [Authorize(Policy = "Teacher.Create")] // SchoolAdmin permission
    public async Task<IActionResult> CreateParent(
        [FromBody] CreateParentCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));

    // ── Guardians (Student ↔ Parent link) ────────────────────────────

    [HttpPost("students/{studentId:guid}/guardians")]
    [Authorize(Policy = "Student.Create")]
    public async Task<IActionResult> LinkGuardian(
        Guid studentId,
        [FromBody] LinkStudentGuardianCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command with { StudentId = studentId }, ct));
}
