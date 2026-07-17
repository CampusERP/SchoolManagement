using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Enrollment.Commands.EnrollStudent;
using Application.Features.Enrollment.Commands.AssignTeacher;
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

    [HttpPost("teachers")]
    [Authorize(Policy = "Schedule.Create")]
    public async Task<IActionResult> AssignTeacher(
        [FromBody] AssignTeacherCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));
}