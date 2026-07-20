using Domain.Common;
using Domain.Exceptions;

namespace Domain.Entities.Billing;

public class Invoice : AuditableEntity
{
    public Guid          SubscriptionId { get; private set; }
    public Guid          SchoolId       { get; private set; }
    public decimal       Amount         { get; private set; }
    public DateTime      DueDate        { get; private set; }
    public InvoiceStatus Status         { get; private set; }
    public string?       Notes          { get; private set; }

    private readonly List<Payment> _payments = new();
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    public decimal PaidAmount    => _payments.Sum(p => p.Amount);
    public decimal BalanceDue    => Amount - PaidAmount;

    private Invoice() { }

    private Invoice(Guid id, Guid subscriptionId, Guid schoolId,
        decimal amount, DateTime dueDate) : base(id)
    {
        SubscriptionId = subscriptionId;
        SchoolId       = schoolId;
        Amount         = amount;
        DueDate        = dueDate;
        Status         = InvoiceStatus.Issued;
    }

    internal static Invoice Create(Guid subscriptionId, Guid schoolId,
        decimal amount, DateTime dueDate)
        => new(Guid.NewGuid(), subscriptionId, schoolId, amount, dueDate);

    public Payment RecordPayment(decimal amount, PaymentMethod method, string? reference = null)
    {
        if (Status == InvoiceStatus.Cancelled)
            throw new DomainException("Cannot record payment on a cancelled invoice.");
        if (amount <= 0)
            throw new DomainException("Payment amount must be positive.");
        if (amount > BalanceDue)
            throw new DomainException($"Payment amount ({amount}) exceeds balance due ({BalanceDue}).");

        var payment = Payment.Create(Id, amount, method, reference);
        _payments.Add(payment);

        if (BalanceDue == 0) Status = InvoiceStatus.Paid;

        return payment;
    }

    public void Cancel()
    {
        if (Status == InvoiceStatus.Paid)
            throw new DomainException("Cannot cancel a paid invoice.");
        Status = InvoiceStatus.Cancelled;
    }
}
