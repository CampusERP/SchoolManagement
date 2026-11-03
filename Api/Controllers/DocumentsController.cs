using Application.Features.Documents.Commands;
using Application.Features.Documents.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
public sealed class DocumentsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public DocumentsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{documentId:guid}")]
    [Authorize(Policy = "Student.Read")]
    public async Task<IActionResult> Get(Guid documentId, [FromQuery] Guid schoolId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDocumentQuery(schoolId, documentId), ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        var document = result.Value;
        if (document is null) return NotFound();
        return File(document.Content, document.ContentType, document.FileName);
    }

    [HttpDelete("{documentId:guid}")]
    [Authorize(Policy = "Student.Update")]
    public async Task<IActionResult> Delete(Guid documentId, [FromQuery] Guid schoolId, CancellationToken ct) =>
        FromResult(await _mediator.Send(new DeleteDocumentCommand(schoolId, documentId), ct));
}
