using Application.Common.Models;
using Application.Features.Billing.Commands;
using Application.Features.Billing.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
public sealed class BillingController(IMediator mediator) : ApiControllerBase
{
    [HttpGet("plans")]
    [Authorize(Policy = "Billing.Read")]
    public async Task<IActionResult> GetPlans(CancellationToken ct) =>
        FromResult(await mediator.Send(new GetSubscriptionPlansQuery(), ct));

    [HttpPost("plans")]
    [Authorize(Policy = "Billing.Manage")]
    public async Task<IActionResult> CreatePlan(CreateSubscriptionPlanCommand command, CancellationToken ct) =>
        Created(await mediator.Send(command, ct));

    [HttpGet("schools/{schoolId:guid}/subscription")]
    [Authorize(Policy = "Billing.Read")]
    public async Task<IActionResult> GetSubscription(Guid schoolId, CancellationToken ct) =>
        FromResult(await mediator.Send(new GetSubscriptionStatusQuery(schoolId), ct));

    [HttpPut("schools/{schoolId:guid}/subscription")]
    [Authorize(Policy = "Billing.Manage")]
    public async Task<IActionResult> AssignSubscription(Guid schoolId, AssignSubscriptionCommand command, CancellationToken ct) =>
        Created(await mediator.Send(command with { SchoolId = schoolId }, ct));

    [HttpPatch("schools/{schoolId:guid}/subscription/upgrade")]
    [Authorize(Policy = "Billing.Manage")]
    public async Task<IActionResult> UpgradeSubscription(Guid schoolId, UpgradeSubscriptionCommand command, CancellationToken ct) =>
        FromResult(await mediator.Send(command with { SchoolId = schoolId }, ct));

    [HttpPatch("schools/{schoolId:guid}/subscription/suspend")]
    [Authorize(Policy = "Billing.Manage")]
    public async Task<IActionResult> SuspendSubscription(Guid schoolId, CancellationToken ct) =>
        FromResult(await mediator.Send(new SuspendSubscriptionCommand(schoolId), ct));

    [HttpDelete("schools/{schoolId:guid}/subscription")]
    [Authorize(Policy = "Billing.Manage")]
    public async Task<IActionResult> CancelSubscription(Guid schoolId, CancellationToken ct) =>
        FromResult(await mediator.Send(new CancelSubscriptionCommand(schoolId), ct));

    [HttpGet("schools/{schoolId:guid}/invoices")]
    [Authorize(Policy = "Billing.Read")]
    public async Task<IActionResult> GetInvoices(Guid schoolId, int page = 1, int pageSize = 20, CancellationToken ct = default) =>
        FromResult(await mediator.Send(new GetInvoicesQuery(schoolId, new PaginationParams(page, pageSize)), ct));

    [HttpPost("schools/{schoolId:guid}/invoices")]
    [Authorize(Policy = "Billing.Manage")]
    public async Task<IActionResult> GenerateInvoice(Guid schoolId, GenerateInvoiceCommand command, CancellationToken ct) =>
        Created(await mediator.Send(command with { SchoolId = schoolId }, ct));

    [HttpPost("invoices/{invoiceId:guid}/payments")]
    [Authorize(Policy = "Billing.Manage")]
    public async Task<IActionResult> RecordPayment(Guid invoiceId, RecordPaymentCommand command, CancellationToken ct) =>
        Created(await mediator.Send(command with { InvoiceId = invoiceId }, ct));
}
