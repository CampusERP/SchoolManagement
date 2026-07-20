using Domain.Common;

namespace Domain.Entities.Billing;

public class Payment : AuditableEntity
{
    public Guid          InvoiceId    { get; private set; }
    public decimal       Amount       { get; private set; }
    public PaymentMethod Method       { get; private set; }
    public string?       Reference    { get; private set; }
    public DateTime      PaidAtUtc    { get; private set; }

    private Payment() { }

    private Payment(Guid id, Guid invoiceId, decimal amount,
        PaymentMethod method, string? reference) : base(id)
    {
        InvoiceId = invoiceId;
        Amount    = amount;
        Method    = method;
        Reference = reference;
        PaidAtUtc = DateTime.UtcNow;
    }

    internal static Payment Create(Guid invoiceId, decimal amount,
        PaymentMethod method, string? reference)
        => new(Guid.NewGuid(), invoiceId, amount, method, reference);
}
