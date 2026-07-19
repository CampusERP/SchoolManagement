using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Subjects.Commands.CreateSubject;
using Application.Features.Academics.Queries.GetSubjects;

namespace Api.Controllers;

[Authorize]
public class SubjectsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public SubjectsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = "AcademicYear.Read")]
    public async Task<IActionResult> GetSubjects(
        [FromQuery] Guid? gradeLevelId, CancellationToken ct) =>
        FromResult(await _mediator.Send(new GetSubjectsQuery(gradeLevelId), ct));

    /// <summary>Add a subject to the global lookup list. Super Admin only.</summary>
    [HttpPost]
    [Authorize(Policy = "School.Manage")]
    public async Task<IActionResult> CreateSubject(
        [FromBody] CreateSubjectCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));

    /// <summary>Assign an existing subject to a grade level for this school. School Admin.</summary>
    [HttpPost("curriculum")]
    [Authorize(Policy = "AcademicYear.Create")]
    public async Task<IActionResult> AddCurriculumSubject(
        [FromBody] AddCurriculumSubjectCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));
}
