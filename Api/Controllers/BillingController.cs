using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Billing.Commands;
using Application.Features.Billing.Queries;
using Application.Common.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
public sealed class BillingController(
    IMediator mediator,
    ITenantContext tenantContext,
    Application.Common.Interfaces.Services.IBillingReadService billingReadService) : ApiControllerBase
{
    // ── Plans ───────────────────────────────────────────────────────

    /// <summary>Public plan catalogue shown on the landing page and registration form.</summary>
    [HttpGet("plans/public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicPlans(CancellationToken ct) =>
        FromResult(await mediator.Send(new GetSubscriptionPlansQuery(ActiveOnly: true), ct));

    [HttpGet("plans")]
    [Authorize(Policy = "Billing.Read")]
    public async Task<IActionResult> GetPlans(CancellationToken ct) =>
        FromResult(await mediator.Send(new GetSubscriptionPlansQuery(), ct));

    [HttpPost("plans")]
    [Authorize(Policy = "Billing.Manage")]
    public async Task<IActionResult> CreatePlan(CreateSubscriptionPlanCommand command, CancellationToken ct) =>
        Created(await mediator.Send(command, ct));

    [HttpPut("plans/{planId:guid}")]
    [Authorize(Policy = "Billing.Manage")]
    public async Task<IActionResult> UpdatePlan(Guid planId, UpdateSubscriptionPlanCommand command, CancellationToken ct) =>
        FromResult(await mediator.Send(command with { PlanId = planId }, ct));

    // ── Subscriptions ───────────────────────────────────────────────

    /// <summary>Platform-wide subscription overview.</summary>
    [HttpGet("subscriptions")]
    [Authorize(Policy = "Billing.Manage")]
    public async Task<IActionResult> GetSubscriptions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? schoolId = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default) =>
        FromResult(await mediator.Send(
            new GetSubscriptionsQuery(new PaginationParams(page, pageSize), schoolId, search), ct));

    [HttpGet("schools/{schoolId:guid}/subscription")]
    [Authorize(Policy = "Billing.Read")]
    public async Task<IActionResult> GetSubscription(Guid schoolId, CancellationToken ct)
    {
        if (!CanAccessSchool(schoolId)) return Forbid();
        return FromResult(await mediator.Send(new GetSubscriptionStatusQuery(schoolId), ct));
    }

    [HttpPut("schools/{schoolId:guid}/subscription")]
    [Authorize(Policy = "Billing.Manage")]
    public async Task<IActionResult> AssignSubscription(
        Guid schoolId, AssignSubscriptionCommand command, CancellationToken ct) =>
        FromResult(await mediator.Send(command with { SchoolId = schoolId }, ct));

    [HttpPatch("schools/{schoolId:guid}/subscription/upgrade")]
    [Authorize(Policy = "Billing.Manage")]
    public async Task<IActionResult> UpgradeSubscription(
        Guid schoolId, UpgradeSubscriptionCommand command, CancellationToken ct) =>
        FromResult(await mediator.Send(command with { SchoolId = schoolId }, ct));

    [HttpPatch("schools/{schoolId:guid}/subscription/suspend")]
    [Authorize(Policy = "Billing.Manage")]
    public async Task<IActionResult> SuspendSubscription(Guid schoolId, CancellationToken ct) =>
        FromResult(await mediator.Send(new SuspendSubscriptionCommand(schoolId), ct));

    [HttpPatch("schools/{schoolId:guid}/subscription/reactivate")]
    [Authorize(Policy = "Billing.Manage")]
    public async Task<IActionResult> ReactivateSubscription(Guid schoolId, CancellationToken ct) =>
        FromResult(await mediator.Send(new ReactivateSubscriptionCommand(schoolId), ct));

    [HttpDelete("schools/{schoolId:guid}/subscription")]
    [Authorize(Policy = "Billing.Manage")]
    public async Task<IActionResult> CancelSubscription(Guid schoolId, CancellationToken ct) =>
        FromResult(await mediator.Send(new CancelSubscriptionCommand(schoolId), ct));

    // ── Invoices ────────────────────────────────────────────────────

    /// <summary>Platform-wide invoice list.</summary>
    [HttpGet("invoices")]
    [Authorize(Policy = "Billing.Manage")]
    public async Task<IActionResult> GetAllInvoices(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? schoolId = null,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        CancellationToken ct = default) =>
        FromResult(await mediator.Send(
            new GetInvoicesQuery(schoolId, new PaginationParams(page, pageSize), search, status), ct));

    [HttpGet("schools/{schoolId:guid}/invoices")]
    [Authorize(Policy = "Billing.Read")]
    public async Task<IActionResult> GetInvoices(
        Guid schoolId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        if (!CanAccessSchool(schoolId)) return Forbid();
        return FromResult(await mediator.Send(
            new GetInvoicesQuery(schoolId, new PaginationParams(page, pageSize), null, status), ct));
    }

    [HttpPost("schools/{schoolId:guid}/invoices")]
    [Authorize(Policy = "Billing.Manage")]
    public async Task<IActionResult> GenerateInvoice(
        Guid schoolId, GenerateInvoiceCommand command, CancellationToken ct) =>
        Created(await mediator.Send(command with { SchoolId = schoolId }, ct));

    [HttpGet("invoices/{invoiceId:guid}")]
    [Authorize(Policy = "Billing.Manage")]
    public async Task<IActionResult> GetInvoiceById(Guid invoiceId, CancellationToken ct) =>
        FromResult(await mediator.Send(new GetInvoiceByIdQuery(invoiceId), ct));

    /// <summary>
    /// Streams an invoice as a PDF. Platform administrators can download any
    /// invoice; a school user may only download invoices for their own school.
    /// </summary>
    [HttpGet("invoices/{invoiceId:guid}/pdf")]
    [Authorize(Policy = "Billing.Read")]
    public async Task<IActionResult> GetInvoicePdf(Guid invoiceId, CancellationToken ct)
    {
        var invoice = await billingReadService.GetInvoiceByIdAsync(invoiceId, ct);
        if (invoice is null) return NotFound();
        if (!CanAccessSchool(invoice.SchoolId)) return Forbid();

        var pdf = InvoicePdfGenerator.Create(new InvoicePdfModel(
            invoice.SchoolName,
            invoice.PlanName,
            invoice.Id.ToString(),
            invoice.Amount,
            invoice.PaidAmount,
            invoice.BalanceDue,
            invoice.DueDate,
            invoice.CreatedAtUtc,
            invoice.Status,
            invoice.Notes,
            invoice.Payments
                .Select(payment => new InvoicePdfPaymentLine(
                    payment.PaidAtUtc,
                    payment.Amount,
                    payment.Method,
                    payment.Reference))
                .ToList()));

        return File(pdf, "application/pdf", $"invoice-{invoice.Id:N}.pdf");
    }

    [HttpPost("invoices/{invoiceId:guid}/payments")]
    [Authorize(Policy = "Billing.Manage")]
    public async Task<IActionResult> RecordPayment(
        Guid invoiceId, RecordPaymentCommand command, CancellationToken ct) =>
        Created(await mediator.Send(command with { InvoiceId = invoiceId }, ct));

    private bool CanAccessSchool(Guid schoolId) =>
        tenantContext.IsPlatformAdmin || tenantContext.SchoolId == schoolId;
}
