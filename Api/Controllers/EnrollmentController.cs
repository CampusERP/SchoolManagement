using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Enrollment.Commands.EnrollStudent;
using Application.Features.Enrollment.Commands.EnrollTeacher;
using Application.Features.Enrollment.Commands.AssignTeacher;
using Application.Features.Enrollment.Commands.UpdateTeachingAssignment;
using Application.Features.Enrollment.Commands.DeleteTeachingAssignment;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[Authorize]
public class EnrollmentController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public EnrollmentController(IMediator mediator) => _mediator = mediator;

    [HttpPost("students")]
    [Authorize(Policy = "Enrollment.Create")]
    public async Task<IActionResult> EnrollStudent(
        [FromBody] EnrollStudentCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));

    [HttpPost("teachers/enroll")]
    [Authorize(Policy = "Enrollment.Create")]
    public async Task<IActionResult> EnrollTeacher(
        [FromBody] EnrollTeacherCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));

    [HttpPost("teachers/assign")]
    [Authorize(Policy = "Schedule.Create")]
    public async Task<IActionResult> AssignTeacher(
        [FromBody] AssignTeacherCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));

    [HttpPut("teachers/assignments/{teachingAssignmentId:guid}")]
    [Authorize(Policy = "Schedule.Create")]
    public async Task<IActionResult> UpdateTeachingAssignment(
        Guid teachingAssignmentId,
        [FromBody] UpdateTeachingAssignmentCommand command, CancellationToken ct) =>
        FromResult(await _mediator.Send(command with { TeachingAssignmentId = teachingAssignmentId }, ct));

    [HttpDelete("teachers/assignments/{teachingAssignmentId:guid}")]
    [Authorize(Policy = "Schedule.Create")]
    public async Task<IActionResult> DeleteTeachingAssignment(
        Guid teachingAssignmentId,
        [FromQuery] Guid schoolId, CancellationToken ct) =>
        FromResult(await _mediator.Send(new DeleteTeachingAssignmentCommand(schoolId, teachingAssignmentId), ct));
}
