namespace Application.Features.Billing.Queries;

public record PaymentDto(Guid Id, decimal Amount, string Method,
    string? Reference, DateTime PaidAtUtc);
