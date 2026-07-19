using Application.Features.Academics.CreateEducationStage;
using Application.Features.Academics.UpdateEducationStage;
using Application.Features.Academics.DeleteEducationStage;
using Application.Features.Academics.Queries.GetEducationStages;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[Route("api/v1/education-stages")]
public class EducationStagesController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public EducationStagesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = "EducationStage.Read")]
    public async Task<IActionResult> GetEducationStages(CancellationToken ct) =>
        FromResult(await _mediator.Send(new GetEducationStagesQuery(), ct));

    [HttpPost]
    [Authorize(Policy = "EducationStage.Create")]
    public async Task<IActionResult> CreateEducationStage(
        [FromBody] CreateEducationStageCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));

    [HttpPut("{educationStageId:guid}")]
    [Authorize(Policy = "EducationStage.Update")]
    public async Task<IActionResult> UpdateEducationStage(
        Guid educationStageId,
        [FromBody] UpdateEducationStageCommand command, CancellationToken ct) =>
        FromResult(await _mediator.Send(command with { EducationStageId = educationStageId }, ct));

    [HttpDelete("{educationStageId:guid}")]
    [Authorize(Policy = "EducationStage.Delete")]
    public async Task<IActionResult> DeleteEducationStage(
        Guid educationStageId, CancellationToken ct) =>
        FromResult(await _mediator.Send(new DeleteEducationStageCommand(educationStageId), ct));
}
