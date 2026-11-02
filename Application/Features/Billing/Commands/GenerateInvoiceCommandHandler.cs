using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Billing.Commands;

public class GenerateInvoiceCommandHandler
    : IRequestHandler<GenerateInvoiceCommand, Result<Guid>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IEmailNotifier _emailNotifier;

    public GenerateInvoiceCommandHandler(
        ISubscriptionRepository subscriptionRepository,
        ISubscriptionPlanRepository planRepository,
        IInvoiceRepository invoiceRepository,
        ISchoolRepository schoolRepository,
        IEmailNotifier emailNotifier)
    {
        _subscriptionRepository = subscriptionRepository;
        _planRepository = planRepository;
        _invoiceRepository = invoiceRepository;
        _schoolRepository = schoolRepository;
        _emailNotifier = emailNotifier;
    }

    public async Task<Result<Guid>> Handle(GenerateInvoiceCommand request, CancellationToken ct)
    {
        var subscription = await _subscriptionRepository.GetLatestBySchoolIdAsync(request.SchoolId, ct);
        if (subscription is null)
            return Result.Failure<Guid>("No subscription found for this school.");

        var plan = await _planRepository.GetByIdAsync(subscription.SubscriptionPlanId, ct);

        // Amount 0 means "bill the current plan's monthly price".
        var amount = request.Amount > 0 ? request.Amount : plan?.PriceMonthly ?? 0m;
        if (amount <= 0)
            return Result.Failure<Guid>("Invoice amount must be positive.");

        var dueDate = request.DueDate?.ToUniversalTime() ?? DateTime.UtcNow.AddDays(7);

        try
        {
            var invoice = subscription.GenerateInvoice(amount, dueDate, request.Notes);

            // The subscription is already tracked, so only the new invoice needs
            // to be explicitly added. Calling Update on the subscription graph
            // would mark the new invoice as Modified and fail on insert.
            _invoiceRepository.Add(invoice);

            var school = await _schoolRepository.GetByIdAsync(request.SchoolId, ct);
            await _emailNotifier.SendToSchoolAdminsAsync(
                request.SchoolId,
                EmailTemplate.InvoiceIssued,
                new Dictionary<string, string?>
                {
                    ["ContactName"] = "School Administrator",
                    ["SchoolName"] = school?.Name ?? "School",
                    ["InvoiceNumber"] = invoice.Id.ToString(),
                    ["PlanName"] = plan?.Name ?? "Subscription",
                    ["Amount"] = $"{invoice.Amount:0.00}",
                    ["DueDate"] = invoice.DueDate.ToString("yyyy-MM-dd")
                },
                ct);

            return Result.Success(invoice.Id);
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return Result.Failure<Guid>(ex.Message);
        }
    }
}
