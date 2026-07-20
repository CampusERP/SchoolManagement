using Domain.Common;
using Domain.Exceptions;

namespace Domain.Entities.Billing;

public class Subscription : AuditableEntity, IAggregateRoot
{
    public Guid               SchoolId             { get; private set; }
    public Guid               SubscriptionPlanId   { get; private set; }
    public SubscriptionStatus Status               { get; private set; }
    public DateTime           StartDate            { get; private set; }
    public DateTime           EndDate              { get; private set; }
    public bool               AutoRenew            { get; private set; }

    private readonly List<Invoice> _invoices = new();
    public IReadOnlyCollection<Invoice> Invoices => _invoices.AsReadOnly();

    private Subscription() { }

    private Subscription(Guid id, Guid schoolId, Guid planId,
        DateTime startDate, DateTime endDate) : base(id)
    {
        SchoolId           = schoolId;
        SubscriptionPlanId = planId;
        Status             = SubscriptionStatus.Active;
        StartDate          = startDate;
        EndDate            = endDate;
        AutoRenew          = true;
    }

    public static Subscription Create(Guid schoolId, Guid planId,
        DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
            throw new DomainException("End date must be after start date.");

        return new Subscription(Guid.NewGuid(), schoolId, planId, startDate, endDate);
    }

    public Invoice GenerateInvoice(decimal amount, DateTime dueDate)
    {
        if (Status == SubscriptionStatus.Cancelled)
            throw new DomainException("Cannot generate invoice for a cancelled subscription.");

        var invoice = Invoice.Create(Id, SchoolId, amount, dueDate);
        _invoices.Add(invoice);
        return invoice;
    }

    public void Suspend()
    {
        if (Status != SubscriptionStatus.Active)
            throw new DomainException("Only active subscriptions can be suspended.");
        Status = SubscriptionStatus.Suspended;
    }

    public void Reactivate()
    {
        if (Status == SubscriptionStatus.Cancelled)
            throw new DomainException("Cancelled subscriptions cannot be reactivated.");
        Status = SubscriptionStatus.Active;
    }

    public void Cancel() => Status = SubscriptionStatus.Cancelled;

    public void Upgrade(Guid newPlanId) => SubscriptionPlanId = newPlanId;
}