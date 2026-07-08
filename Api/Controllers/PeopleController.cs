using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Features.People.Commands.CreateStudent;
using Application.Features.People.Commands.CreateTeacher;
using Application.Features.People.Commands.CreateParent;
using Application.Features.People.Commands.LinkStudentGuardian;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PeopleController : ControllerBase
{
    private readonly IMediator _mediator;

    public PeopleController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("students")]
    public async Task<IActionResult> CreateStudent(CreateStudentCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(CreateStudent), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPost("teachers")]
    public async Task<IActionResult> CreateTeacher(CreateTeacherCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(CreateTeacher), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPost("parents")]
    public async Task<IActionResult> CreateParent(CreateParentCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(CreateParent), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPost("student-guardians")]
    public async Task<IActionResult> LinkStudentGuardian(LinkStudentGuardianCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(LinkStudentGuardian), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }
}
