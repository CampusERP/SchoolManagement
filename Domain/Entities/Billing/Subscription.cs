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

    /// <summary>Creates a subscription that already has an active paid period.</summary>
    public static Subscription Create(Guid schoolId, Guid planId,
        DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
            throw new DomainException("End date must be after start date.");

        return new Subscription(Guid.NewGuid(), schoolId, planId, startDate, endDate);
    }

    /// <summary>
    /// Creates a subscription that is waiting for its first invoice to be paid.
    /// The real period starts automatically when the invoice is settled.
    /// </summary>
    public static Subscription CreatePendingPayment(Guid schoolId, Guid planId, DateTime now)
    {
        var subscription = new Subscription(
            Guid.NewGuid(), schoolId, planId, now, now.AddMonths(1));

        subscription.Status = SubscriptionStatus.PendingPayment;
        return subscription;
    }

    public Invoice GenerateInvoice(decimal amount, DateTime dueDate, string? notes = null)
    {
        if (Status == SubscriptionStatus.Cancelled)
            throw new DomainException("Cannot generate invoice for a cancelled subscription.");
        if (amount <= 0)
            throw new DomainException("Invoice amount must be positive.");

        var invoice = Invoice.Create(Id, SchoolId, amount, dueDate, notes);
        _invoices.Add(invoice);
        return invoice;
    }

    /// <summary>Starts a fresh period (or resets an expired one) once payment is received.</summary>
    public void ActivatePeriod(DateTime startUtc, int months = 1)
    {
        if (months <= 0)
            throw new DomainException("Subscription period must be at least one month.");
        if (Status == SubscriptionStatus.Cancelled)
            throw new DomainException("Cancelled subscriptions cannot be reactivated.");

        StartDate = startUtc;
        EndDate   = startUtc.AddMonths(months);
        Status    = SubscriptionStatus.Active;
    }

    /// <summary>Extends a still-running period (renewal) when a renewal invoice is paid.</summary>
    public void ExtendPeriod(int months = 1)
    {
        if (months <= 0)
            throw new DomainException("Subscription period must be at least one month.");
        if (Status == SubscriptionStatus.Cancelled)
            throw new DomainException("Cancelled subscriptions cannot be renewed.");

        var basis = EndDate > DateTime.UtcNow ? EndDate : DateTime.UtcNow;
        StartDate = StartDate > DateTime.UtcNow ? StartDate : DateTime.UtcNow;
        EndDate   = basis.AddMonths(months);
        Status    = SubscriptionStatus.Active;
    }

    /// <summary>Returns true when the subscription has a paid, currently running period.</summary>
    public bool IsInPeriod(DateTime utcNow) =>
        Status == SubscriptionStatus.Active && utcNow <= EndDate;

    /// <summary>Returns true when an active subscription's period has lapsed.</summary>
    public bool IsPeriodOver(DateTime utcNow) =>
        Status == SubscriptionStatus.Active && utcNow > EndDate;

    public void Expire() => Status = SubscriptionStatus.Expired;

    public void Suspend()
    {
        if (Status != SubscriptionStatus.Active)
            throw new DomainException("Only active subscriptions can be suspended.");
        Status = SubscriptionStatus.Suspended;
    }

    /// <summary>Reactivates a suspended subscription. Expired periods are restarted from now.</summary>
    public void Reactivate(DateTime utcNow)
    {
        if (Status == SubscriptionStatus.Cancelled)
            throw new DomainException("Cancelled subscriptions cannot be reactivated.");

        if (EndDate <= utcNow)
        {
            StartDate = utcNow;
            EndDate   = utcNow.AddMonths(1);
        }

        Status = SubscriptionStatus.Active;
    }

    public void Cancel() => Status = SubscriptionStatus.Cancelled;

    public void Upgrade(Guid newPlanId)
    {
        if (newPlanId == Guid.Empty)
            throw new DomainException("A valid subscription plan is required.");
        SubscriptionPlanId = newPlanId;
    }

    /// <summary>Platform-side edit of the plan and/or the covered period.</summary>
    public void ChangePlan(Guid newPlanId, DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
            throw new DomainException("End date must be after start date.");

        SubscriptionPlanId = newPlanId;
        StartDate          = startDate;
        EndDate            = endDate;
        if (Status == SubscriptionStatus.Expired || Status == SubscriptionStatus.PendingPayment)
            Status = SubscriptionStatus.Active;
    }
}
