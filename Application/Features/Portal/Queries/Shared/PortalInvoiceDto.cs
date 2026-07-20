namespace Application.Features.Portal.Queries.Shared;

public record PortalInvoiceDto(
    Guid     Id,
    decimal  Amount,
    decimal  PaidAmount,
    decimal  BalanceDue,
    DateTime DueDate,
    string   Status,
    DateTime CreatedAtUtc);
