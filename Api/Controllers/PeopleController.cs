using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Features.People.Commands.CreateStudent;
using Application.Features.People.Commands.CreateTeacher;
using Application.Features.People.Commands.CreateParent;
using Application.Features.People.Commands.LinkStudentGuardian;
using Application.Features.People.Commands.UpdateStudent;
using Application.Features.People.Commands.UpdateTeacher;
using Application.Features.People.Commands.UpdateParent;
using Microsoft.AspNetCore.Authorization;
using Application.Features.People.Queries.GetStudents;
using Application.Features.People.Queries.StudentDetails;
using Application.Features.People.Queries.GetTeachers;
using Application.Features.People.Queries.GetTeacherById;
using Application.Features.People.Queries.GetParents;
using Application.Features.People.Queries.GetParentById;
using Application.Features.People.Queries.GetMyChildren;
using Application.Features.People.Queries.GetMyClasses;

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
        FromResult(await _mediator.Send(query, ct));

    [HttpGet("students/{studentId:guid}")]
    [Authorize(Policy = "Student.Read")]
    public async Task<IActionResult> GetStudentById(
        Guid studentId, [FromQuery] Guid schoolId, CancellationToken ct) =>
        FromResult(await _mediator.Send(new GetStudentByIdQuery(schoolId, studentId), ct));

    [HttpPost("students")]
    [Authorize(Policy = "Student.Create")]
    public async Task<IActionResult> CreateStudent(
        [FromBody] CreateStudentCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));

    [HttpPut("students/{studentId:guid}")]
    [Authorize(Policy = "Student.Update")]
    public async Task<IActionResult> UpdateStudent(
        Guid studentId,
        [FromBody] UpdateStudentCommand command, CancellationToken ct) =>
        FromResult(await _mediator.Send(command with { StudentId = studentId }, ct));

    // ── Teachers ──────────────────────────────────────────────────────

    [HttpPost("teachers")]
    [Authorize(Policy = "Teacher.Create")]
    public async Task<IActionResult> CreateTeacher(
        [FromBody] CreateTeacherCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));

    [HttpGet("teachers")]
    [Authorize(Policy = "Teacher.Read")]
    public async Task<IActionResult> GetTeachers(
        [FromQuery] GetTeachersQuery query, CancellationToken ct) =>
        FromResult(await _mediator.Send(query, ct));

    [HttpGet("teachers/{teacherId:guid}")]
    [Authorize(Policy = "Teacher.Read")]
    public async Task<IActionResult> GetTeacherById(
        Guid teacherId, [FromQuery] Guid schoolId, CancellationToken ct) =>
        FromResult(await _mediator.Send(new GetTeacherByIdQuery(schoolId, teacherId), ct));

    [HttpPut("teachers/{teacherId:guid}")]
    [Authorize(Policy = "Teacher.Update")]
    public async Task<IActionResult> UpdateTeacher(
        Guid teacherId, [FromBody] UpdateTeacherCommand command, CancellationToken ct) =>
        FromResult(await _mediator.Send(command with { TeacherId = teacherId }, ct));

    // ── Parents ───────────────────────────────────────────────────────

    [HttpPost("parents")]
    [Authorize(Policy = "Parent.Create")]
    public async Task<IActionResult> CreateParent(
        [FromBody] CreateParentCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));

    [HttpGet("parents")]
    [Authorize(Policy = "Parent.Read")]
    public async Task<IActionResult> GetParents(
        [FromQuery] GetParentsQuery query, CancellationToken ct) =>
        FromResult(await _mediator.Send(query, ct));

    [HttpGet("parents/{parentId:guid}")]
    [Authorize(Policy = "Parent.Read")]
    public async Task<IActionResult> GetParentById(
        Guid parentId, [FromQuery] Guid schoolId, CancellationToken ct) =>
        FromResult(await _mediator.Send(new GetParentByIdQuery(schoolId, parentId), ct));

    [HttpPut("parents/{parentId:guid}")]
    [Authorize(Policy = "Parent.Update")]
    public async Task<IActionResult> UpdateParent(
        Guid parentId, [FromBody] UpdateParentCommand command, CancellationToken ct) =>
        FromResult(await _mediator.Send(command with { ParentId = parentId }, ct));

    // ── Guardians (Student ↔ Parent link) ────────────────────────────

    [HttpPost("students/{studentId:guid}/guardians")]
    [Authorize(Policy = "Student.Create")]
    public async Task<IActionResult> LinkGuardian(
        Guid studentId,
        [FromBody] LinkStudentGuardianCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command with { StudentId = studentId }, ct));

    // â”€â”€ Current-user views â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [HttpGet("me/student-profile")]
    [Authorize(Policy = "Profile.Read")]
    public async Task<IActionResult> GetMyStudentProfile(
        [FromQuery] Guid schoolId, CancellationToken ct) =>
        FromResult(await _mediator.Send(new GetMyProfileQuery(schoolId), ct));

    [HttpGet("me/children")]
    [Authorize(Policy = "Children.Read")]
    public async Task<IActionResult> GetMyChildren(
        [FromQuery] Guid schoolId, CancellationToken ct) =>
        FromResult(await _mediator.Send(new GetMyChildrenQuery(schoolId), ct));

    [HttpGet("me/classes")]
    [Authorize(Policy = "MyClasses.Read")]
    public async Task<IActionResult> GetMyClasses(
        [FromQuery] Guid schoolId, [FromQuery] Guid termId, CancellationToken ct) =>
        FromResult(await _mediator.Send(new GetMyClassesQuery(schoolId, termId), ct));
}
