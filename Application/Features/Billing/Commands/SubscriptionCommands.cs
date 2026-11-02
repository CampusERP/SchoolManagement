using Application.Common.Models;
using MediatR;

namespace Application.Features.Billing.Commands;

public record AssignSubscriptionCommand(
    Guid SchoolId, Guid SubscriptionPlanId,
    DateTime? StartDate = null, DateTime? EndDate = null)
    : ICommand<Guid>;

public record UpgradeSubscriptionCommand(Guid SchoolId, Guid NewPlanId) : ICommand;

public record SuspendSubscriptionCommand(Guid SchoolId) : ICommand;

public record ReactivateSubscriptionCommand(Guid SchoolId, DateTime? EndDate = null) : ICommand;

public record CancelSubscriptionCommand(Guid SchoolId) : ICommand;

public record GenerateInvoiceCommand(
    Guid SchoolId, decimal Amount, DateTime? DueDate = null, string? Notes = null)
    : ICommand<Guid>;

public record RecordPaymentCommand(
    Guid InvoiceId, decimal Amount, Domain.Entities.Billing.PaymentMethod Method, string? Reference = null)
    : ICommand<Guid>;
