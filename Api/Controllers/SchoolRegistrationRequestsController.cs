using Application.Features.SchoolRegistrationRequests.Commands.ApproveSchoolRegistrationRequest;
using Application.Features.SchoolRegistrationRequests.Commands.CreateSchoolRegistrationRequest;
using Application.Features.SchoolRegistrationRequests.Commands.DeleteSchoolRegistrationRequest;
using Application.Features.SchoolRegistrationRequests.Commands.RejectSchoolRegistrationRequest;
using Application.Features.SchoolRegistrationRequests.Commands.UpdateSchoolRegistrationRequest;
using Application.Features.SchoolRegistrationRequests.Queries.GetSchoolRegistrationRequestById;
using Application.Features.SchoolRegistrationRequests.Queries.GetSchoolRegistrationRequests;
using Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Public and platform-admin endpoints for school registration requests.
/// </summary>
public class SchoolRegistrationRequestsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public SchoolRegistrationRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ── Public ──────────────────────────────────────────────────────────

    /// <summary>
    /// Submit a new school registration request. Anonymous.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateRequest(
        [FromBody] CreateSchoolRegistrationRequestCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Created(result);
    }

    // ── Platform Admin (Super Admin) ─────────────────────────────────────

    /// <summary>
    /// List school registration requests. Platform super admin only.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "School.Manage")]
    public async Task<IActionResult> GetRequests(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new GetSchoolRegistrationRequestsQuery(
            search, status, new PaginationParams(page, pageSize));
        return FromResult(await _mediator.Send(query, ct));
    }

    /// <summary>
    /// Get a single school registration request by id. Platform super admin only.
    /// </summary>
    [HttpGet("{requestId:guid}")]
    [Authorize(Policy = "School.Manage")]
    public async Task<IActionResult> GetRequestById(Guid requestId, CancellationToken ct)
    {
        var query = new GetSchoolRegistrationRequestByIdQuery(requestId);
        return FromResult(await _mediator.Send(query, ct));
    }

    /// <summary>
    /// Update a pending school registration request. Platform super admin only.
    /// </summary>
    [HttpPut("{requestId:guid}")]
    [Authorize(Policy = "School.Manage")]
    public async Task<IActionResult> UpdateRequest(
        Guid requestId,
        [FromBody] UpdateSchoolRegistrationRequestCommand command,
        CancellationToken ct)
    {
        var cmd = command with { RequestId = requestId };
        return FromResult(await _mediator.Send(cmd, ct));
    }

    /// <summary>
    /// Delete a school registration request. Platform super admin only.
    /// </summary>
    [HttpDelete("{requestId:guid}")]
    [Authorize(Policy = "School.Manage")]
    public async Task<IActionResult> DeleteRequest(Guid requestId, CancellationToken ct)
    {
        var cmd = new DeleteSchoolRegistrationRequestCommand(requestId);
        return FromResult(await _mediator.Send(cmd, ct));
    }

    /// <summary>
    /// Approve a pending school registration request. Creates the school and school admin.
    /// Platform super admin only.
    /// </summary>
    [HttpPost("{requestId:guid}/approve")]
    [Authorize(Policy = "School.Manage")]
    public async Task<IActionResult> ApproveRequest(
        Guid requestId,
        [FromBody] ApproveSchoolRegistrationRequestBody body,
        CancellationToken ct)
    {
        var cmd = new ApproveSchoolRegistrationRequestCommand(requestId, body?.Notes);
        return FromResult(await _mediator.Send(cmd, ct));
    }

    /// <summary>
    /// Reject a pending school registration request. Platform super admin only.
    /// </summary>
    [HttpPost("{requestId:guid}/reject")]
    [Authorize(Policy = "School.Manage")]
    public async Task<IActionResult> RejectRequest(
        Guid requestId,
        [FromBody] RejectSchoolRegistrationRequestBody body,
        CancellationToken ct)
    {
        var cmd = new RejectSchoolRegistrationRequestCommand(requestId, body?.Reason);
        return FromResult(await _mediator.Send(cmd, ct));
    }

    public record ApproveSchoolRegistrationRequestBody(string? Notes);

    public record RejectSchoolRegistrationRequestBody(string? Reason);
}
