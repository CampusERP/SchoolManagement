using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Subjects.Commands.CreateSubject;
using Application.Features.Academics.Queries.GetSubjects;
using Application.Features.Subjects.Queries.GetGradeCurriculum;
using Application.Features.Subjects.Commands.RemoveCurriculumSubject;
using Application.Features.Subjects.Commands.ManageSubjectBooks;
using Application.Features.Subjects.Commands.ManageSchoolSubjects;

namespace Api.Controllers;

[Authorize]
public class SubjectsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public SubjectsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = "AcademicYear.Read")]
    public async Task<IActionResult> GetSubjects(
        [FromQuery] Guid schoolId, [FromQuery] Guid? gradeLevelId, CancellationToken ct) =>
        FromResult(await _mediator.Send(new GetSubjectsQuery(schoolId, gradeLevelId), ct));

    [HttpGet("curriculum/{gradeLevelId:guid}")]
    [Authorize(Policy = "AcademicYear.Read")]
    public async Task<IActionResult> GetGradeCurriculum(
        Guid gradeLevelId, [FromQuery] Guid schoolId, CancellationToken ct) =>
        FromResult(await _mediator.Send(new GetGradeCurriculumQuery(schoolId, gradeLevelId), ct));

    /// <summary>Add a subject to the shared catalog. Super Admin only.</summary>
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

    [HttpPost("school-subjects")]
    [Authorize(Policy = "AcademicYear.Create")]
    public async Task<IActionResult> CreateSchoolSubject(
        [FromBody] CreateSchoolSubjectCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command, ct));

    [HttpPut("curriculum/{curriculumSubjectId:guid}/subject")]
    [Authorize(Policy = "AcademicYear.Update")]
    public async Task<IActionResult> UpdateSchoolSubject(
        Guid curriculumSubjectId, [FromBody] UpdateSchoolSubjectCommand command, CancellationToken ct) =>
        FromResult(await _mediator.Send(command with { CurriculumSubjectId = curriculumSubjectId }, ct));

    [HttpDelete("school-subjects/{subjectId:guid}")]
    [Authorize(Policy = "AcademicYear.Update")]
    public async Task<IActionResult> DeleteSchoolSubject(
        Guid subjectId, [FromQuery] Guid schoolId, CancellationToken ct) =>
        FromResult(await _mediator.Send(new DeleteSchoolSubjectCommand(schoolId, subjectId), ct));

    [HttpDelete("curriculum/{curriculumSubjectId:guid}")]
    [Authorize(Policy = "AcademicYear.Update")]
    public async Task<IActionResult> RemoveCurriculumSubject(
        Guid curriculumSubjectId, [FromQuery] Guid schoolId, CancellationToken ct) =>
        FromResult(await _mediator.Send(new RemoveCurriculumSubjectCommand(schoolId, curriculumSubjectId), ct));

    [HttpPost("curriculum/{curriculumSubjectId:guid}/books")]
    [Authorize(Policy = "AcademicYear.Update")]
    public async Task<IActionResult> CreateSubjectBook(
        Guid curriculumSubjectId, [FromBody] CreateSubjectBookCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command with { CurriculumSubjectId = curriculumSubjectId }, ct));

    [HttpPut("curriculum/{curriculumSubjectId:guid}/books/{bookId:guid}")]
    [Authorize(Policy = "AcademicYear.Update")]
    public async Task<IActionResult> UpdateSubjectBook(
        Guid curriculumSubjectId, Guid bookId, [FromBody] UpdateSubjectBookCommand command, CancellationToken ct) =>
        FromResult(await _mediator.Send(command with { CurriculumSubjectId = curriculumSubjectId, BookId = bookId }, ct));

    [HttpDelete("curriculum/{curriculumSubjectId:guid}/books/{bookId:guid}")]
    [Authorize(Policy = "AcademicYear.Update")]
    public async Task<IActionResult> DeleteSubjectBook(
        Guid curriculumSubjectId, Guid bookId, [FromQuery] Guid schoolId, CancellationToken ct) =>
        FromResult(await _mediator.Send(new DeleteSubjectBookCommand(schoolId, curriculumSubjectId, bookId), ct));
}
