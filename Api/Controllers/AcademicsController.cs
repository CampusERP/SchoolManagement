using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Academics.CreateAcademicYear;
using Application.Features.Academics.CreateClassRoom;
using Application.Features.Academics.CreateGradeLevel;
using Application.Features.Academics.CreateRoom;
using Application.Features.Academics.CreateTerm;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AcademicsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AcademicsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("academic-years")]
    public async Task<IActionResult> CreateAcademicYear(CreateAcademicYearCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(CreateAcademicYear), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPost("classrooms")]
    public async Task<IActionResult> CreateClassRoom(CreateClassRoomCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(CreateClassRoom), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPost("grade-levels")]
    public async Task<IActionResult> CreateGradeLevel(CreateGradeLevelCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(CreateGradeLevel), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPost("rooms")]
    public async Task<IActionResult> CreateRoom(CreateRoomCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(CreateRoom), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPost("terms")]
    public async Task<IActionResult> CreateTerm(CreateTermCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(CreateTerm), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }
}
