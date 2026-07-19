using Application.Features.Schools.Commands.CreateSchool;
using Application.Features.Schools.Commands.UpdateSchool;
using Application.Features.Schools.Commands.AddCampus;
using Application.Features.Schools.Commands.SchoolActivation;
using Application.Features.Schools.Queries.GetAllSchools;
using Application.Features.Schools.Queries.GetPlatformAnalytics;
using Application.Features.Schools.Queries.GetSchoolById;
using Application.Features.Schools.Queries.GetSchoolDashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
public class SchoolsController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public SchoolsController(IMediator mediator) => _mediator = mediator;

    // ── Platform-level (Super Admin) ──────────────────────────────────

    [HttpGet]
    [Authorize(Policy = "School.Read")]
    public async Task<IActionResult> GetAllSchools(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        FromResult(await _mediator.Send(
            new GetAllSchoolsQuery(new Application.Common.Models.PaginationParams(page, pageSize)), ct));

    [HttpGet("analytics")]
    [Authorize(Policy = "Platform.Analytics")]
    public async Task<IActionResult> GetPlatformAnalytics(CancellationToken ct) =>
        FromResult(await _mediator.Send(new GetPlatformAnalyticsQuery(), ct));

    [HttpGet("{schoolId:guid}")]
    [Authorize(Policy = "School.Read")]
    public async Task<IActionResult> GetSchoolById(Guid schoolId, CancellationToken ct) =>
        FromResult(await _mediator.Send(new GetSchoolByIdQuery(schoolId), ct));

    [HttpGet("{schoolId:guid}/dashboard")]
    [Authorize(Policy = "School.Dashboard")]
    public async Task<IActionResult> GetSchoolDashboard(Guid schoolId, CancellationToken ct) =>
        FromResult(await _mediator.Send(new GetSchoolDashboardQuery(schoolId), ct));

    [HttpPost]
    [Authorize(Policy = "School.Create")]
    public async Task<IActionResult> CreateSchool(
        [FromBody] CreateSchoolCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Created(result);
    }

    [HttpPut("{schoolId:guid}")]
    [Authorize(Policy = "School.Update")]
    public async Task<IActionResult> UpdateSchool(
        Guid schoolId,
        [FromBody] UpdateSchoolCommand command, CancellationToken ct) =>
        FromResult(await _mediator.Send(command with { SchoolId = schoolId }, ct));

    [HttpPost("{schoolId:guid}/campuses")]
    [Authorize(Policy = "School.Update")]
    public async Task<IActionResult> AddCampus(
        Guid schoolId, [FromBody] AddCampusCommand command, CancellationToken ct) =>
        Created(await _mediator.Send(command with { SchoolId = schoolId }, ct));

    [HttpPost("{schoolId:guid}/suspend")]
    [Authorize(Policy = "School.Update")]
    public async Task<IActionResult> SuspendSchool(Guid schoolId, CancellationToken ct) =>
        FromResult(await _mediator.Send(new SuspendSchoolCommand(schoolId), ct));

    [HttpPost("{schoolId:guid}/reactivate")]
    [Authorize(Policy = "School.Update")]
    public async Task<IActionResult> ReactivateSchool(Guid schoolId, CancellationToken ct) =>
        FromResult(await _mediator.Send(new ReactivateSchoolCommand(schoolId), ct));
}
