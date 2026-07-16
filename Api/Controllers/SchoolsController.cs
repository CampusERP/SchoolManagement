using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Schools.Commands.CreateSchool;

namespace Api.Controllers;

[Authorize(Policy = "School.Create")]
public class SchoolsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public SchoolsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> CreateSchool(
        [FromBody] CreateSchoolCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Created(result);
    }
}
