using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Common.Models;
using Application.Features.Assignments.Commands.CreateAssignment;
using Application.Features.Assignments.Queries.GetClassAssignments;

namespace Api.Controllers;

[Authorize]
public class AssignmentsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public AssignmentsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Creates a new assignment for a teaching assignment. Teacher only.</summary>
    [HttpPost]
    [Authorize(Policy = "Assignment.Create")]
    public async Task<IActionResult> CreateAssignment(
        [FromBody] CreateAssignmentCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));

    /// <summary>
    /// Submit an assignment (student). Accepts multipart/form-data for file uploads.
    /// Late detection is automatic — the domain compares UtcNow to DueDate.
    /// </summary>
    [HttpPost("{assignmentId:guid}/submissions")]
    [Authorize(Policy = "Assignment.Submit")]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB per submission
    public async Task<IActionResult> SubmitAssignment(
        Guid assignmentId,
        [FromForm] SubmitAssignmentCommand command,
        [FromForm] List<IFormFile>? files,
        CancellationToken ct) =>
        Created(await _mediator.Send(command with
        {
            AssignmentId = assignmentId,
            Files = files?.Select(file => new SubmissionFile(
                file.OpenReadStream(), file.FileName, file.ContentType)).ToList()
        }, ct));

    /// <summary>Grade a student submission. Teacher only.</summary>
    [HttpPatch("{assignmentId:guid}/submissions/{submissionId:guid}/grade")]
    [Authorize(Policy = "Grade.Enter")]
    public async Task<IActionResult> GradeSubmission(
        Guid assignmentId,
        Guid submissionId,
        [FromBody] GradeSubmissionCommand command,
        CancellationToken ct) =>
        FromResult(await _mediator.Send(
            command with { AssignmentId = assignmentId, SubmissionId = submissionId }, ct));

    /// <summary>Get all assignments for a class (teacher view), paginated.</summary>
    [HttpGet("class/{teachingAssignmentId:guid}")]
    [Authorize(Policy = "Assignment.Read")]
    public async Task<IActionResult> GetClassAssignments(
        Guid teachingAssignmentId,
        [FromQuery] Guid schoolId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        FromResult(await _mediator.Send(
            new GetClassAssignmentsQuery(schoolId, teachingAssignmentId,
                new PaginationParams(page, pageSize)), ct));

    /// <summary>Get all assignments visible to a student (student portal), paginated.</summary>
    [HttpGet("student/{enrollmentId:guid}")]
    [Authorize(Policy = "Assignment.ReadOwn")]
    public async Task<IActionResult> GetStudentAssignments(
        Guid enrollmentId,
        [FromQuery] Guid schoolId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        FromResult(await _mediator.Send(
            new GetStudentAssignmentsQuery(schoolId, enrollmentId,
                new PaginationParams(page, pageSize)), ct));
}
