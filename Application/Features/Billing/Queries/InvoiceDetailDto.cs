namespace Application.Features.Billing.Queries;

public record InvoiceDetailDto(
    Guid Id,
    Guid SubscriptionId,
    Guid SchoolId,
    string SchoolName,
    string PlanName,
    decimal Amount,
    decimal PaidAmount,
    decimal BalanceDue,
    DateTime DueDate,
    string Status,
    string? Notes,
    DateTime CreatedAtUtc,
    List<PaymentDto> Payments);
