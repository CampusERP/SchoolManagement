using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Domain.Entities.Billing;
using MediatR;

namespace Application.Features.Billing.Commands;

public class RecordPaymentCommandHandler
    : IRequestHandler<RecordPaymentCommand, Result<Guid>>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IEmailNotifier _emailNotifier;

    public RecordPaymentCommandHandler(
        IInvoiceRepository invoiceRepository,
        ISubscriptionRepository subscriptionRepository,
        IPaymentRepository paymentRepository,
        ISchoolRepository schoolRepository,
        IEmailNotifier emailNotifier)
    {
        _invoiceRepository = invoiceRepository;
        _subscriptionRepository = subscriptionRepository;
        _paymentRepository = paymentRepository;
        _schoolRepository = schoolRepository;
        _emailNotifier = emailNotifier;
    }

    public async Task<Result<Guid>> Handle(RecordPaymentCommand request, CancellationToken ct)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, ct);
        if (invoice is null)
            throw new NotFoundException(nameof(Invoice), request.InvoiceId);

        try
        {
            var payment = invoice.RecordPayment(request.Amount, request.Method, request.Reference);

            // The invoice is tracked; add the new payment explicitly so EF does
            // not interpret it as an existing row through Update(invoice).
            _paymentRepository.Add(payment);

            Subscription? activatedSubscription = null;

            // A fully settled invoice starts (or extends) the paid period automatically.
            if (invoice.Status == InvoiceStatus.Paid)
            {
                var subscription = await _subscriptionRepository.GetByIdAsync(invoice.SubscriptionId, ct);
                if (subscription is not null)
                {
                    activatedSubscription = subscription;
                    var now = DateTime.UtcNow;

                    if (subscription.Status is SubscriptionStatus.PendingPayment or SubscriptionStatus.Expired)
                        subscription.ActivatePeriod(now);
                    else if (subscription.Status == SubscriptionStatus.Active)
                        subscription.ExtendPeriod();
                    // Suspended / Cancelled subscriptions keep their status; the period
                    // is only extended once a platform admin reactivates them.

                    // The subscription is already tracked by the same DbContext, so
                    // its changes are persisted with the invoice update and the new
                    // payment. Calling Update() here would walk the graph and mark the
                    // newly-added payment as Modified instead of Added.
                }
            }

            var school = await _schoolRepository.GetByIdAsync(invoice.SchoolId, ct);
            var statusMessage = invoice.Status == InvoiceStatus.Paid
                ? activatedSubscription?.Status == SubscriptionStatus.Active
                    ? "Your subscription period has been activated/extended."
                    : "Invoice paid in full. If your subscription is suspended or cancelled, contact the platform team to restore access."
                : $"Remaining balance: {invoice.BalanceDue:0.00}.";

            await _emailNotifier.SendToSchoolAdminsAsync(
                invoice.SchoolId,
                EmailTemplate.PaymentReceived,
                new Dictionary<string, string?>
                {
                    ["ContactName"] = "School Administrator",
                    ["SchoolName"] = school?.Name ?? "School",
                    ["Amount"] = $"{request.Amount:0.00}",
                    ["Method"] = request.Method.ToString(),
                    ["Reference"] = string.IsNullOrWhiteSpace(request.Reference) ? "N/A" : request.Reference,
                    ["InvoiceNumber"] = invoice.Id.ToString(),
                    ["BalanceDue"] = $"{invoice.BalanceDue:0.00}",
                    ["StatusMessage"] = statusMessage
                },
                ct);

            return Result.Success(payment.Id);
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return Result.Failure<Guid>(ex.Message);
        }
    }
}
