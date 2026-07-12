using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Enrollment.Commands.EnrollStudent;
using Application.Features.Enrollment.Commands.AssignTeacher;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentController : ControllerBase
{
    private readonly IMediator _mediator;

    public EnrollmentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("students")]
    public async Task<IActionResult> EnrollStudent(EnrollStudentCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(EnrollStudent), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPost("teaching-assignments")]
    public async Task<IActionResult> AssignTeacher(AssignTeacherCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(AssignTeacher), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }
}
