namespace Application.Features.Billing.Queries;

public record InvoiceDetailDto(Guid Id, decimal Amount, decimal PaidAmount,
    decimal BalanceDue, DateTime DueDate, string Status,
    List<PaymentDto> Payments);
