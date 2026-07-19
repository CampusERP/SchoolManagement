namespace Application.Features.Billing.Queries;

public record InvoiceListDto(Guid Id, decimal Amount, decimal PaidAmount,
    decimal BalanceDue, DateTime DueDate, string Status, DateTime CreatedAtUtc);
