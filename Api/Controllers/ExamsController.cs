using Application.Common.Models;
using Application.Features.Exams.Commands;
using Application.Features.Exams.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
public sealed class ExamsController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Exam.Read")]
    public async Task<IActionResult> GetExams(Guid schoolId, Guid termId, int page = 1, int pageSize = 20, CancellationToken ct = default) =>
        FromResult(await mediator.Send(new GetExamsQuery(schoolId, termId, new PaginationParams(page, pageSize)), ct));

    [HttpPost]
    [Authorize(Policy = "Exam.Create")]
    public async Task<IActionResult> Create(CreateExamCommand command, CancellationToken ct) => Created(await mediator.Send(command, ct));

    [HttpGet("{examId:guid}/schedules")]
    [Authorize(Policy = "Exam.Read")]
    public async Task<IActionResult> GetExamSchedules(Guid examId, CancellationToken ct) =>
        FromResult(await mediator.Send(new GetExamSchedulesQuery(examId), ct));

    [HttpPost("{examId:guid}/schedules")]
    [Authorize(Policy = "Exam.Create")]
    public async Task<IActionResult> AddSchedule(Guid examId, AddExamScheduleCommand command, CancellationToken ct) => Created(await mediator.Send(command with { ExamId = examId }, ct));

    [HttpPost("{examId:guid}/results")]
    [Authorize(Policy = "Grade.Enter")]
    public async Task<IActionResult> RecordResults(Guid examId, RecordExamResultsCommand command, CancellationToken ct) => FromResult(await mediator.Send(command with { ExamId = examId }, ct));

    [HttpPatch("{examId:guid}/lock")]
    [Authorize(Policy = "Exam.Manage")]
    public async Task<IActionResult> Lock(Guid examId, Guid schoolId, CancellationToken ct) => FromResult(await mediator.Send(new LockExamCommand(schoolId, examId), ct));

    [HttpGet("schedules/{examScheduleId:guid}/results")]
    [Authorize(Policy = "Exam.Read")]
    public async Task<IActionResult> GetClassResults(Guid examScheduleId, Guid schoolId, CancellationToken ct) => FromResult(await mediator.Send(new GetClassExamResultsQuery(schoolId, examScheduleId), ct));

    [HttpGet("students/{enrollmentId:guid}/results")]
    [Authorize(Policy = "Exam.Read")]
    public async Task<IActionResult> GetStudentResults(Guid enrollmentId, Guid schoolId, Guid? termId = null, int page = 1, int pageSize = 20, CancellationToken ct = default) => FromResult(await mediator.Send(new GetStudentExamResultsQuery(schoolId, enrollmentId, termId, new PaginationParams(page, pageSize)), ct));

    [HttpGet("students/{enrollmentId:guid}/report-cards")]
    [Authorize(Policy = "Exam.Read")]
    public async Task<IActionResult> GetReportCards(Guid enrollmentId, Guid schoolId, int page = 1, int pageSize = 20, CancellationToken ct = default) => FromResult(await mediator.Send(new GetReportCardsQuery(schoolId, enrollmentId, new PaginationParams(page, pageSize)), ct));

    [HttpPost("students/{enrollmentId:guid}/report-cards")]
    [Authorize(Policy = "Exam.Manage")]
    public async Task<IActionResult> GenerateReportCard(Guid enrollmentId, GenerateReportCardCommand command, CancellationToken ct) => Created(await mediator.Send(command with { StudentEnrollmentId = enrollmentId }, ct));
}
