using Domain.Common;

namespace Domain.Entities.Billing;

public enum SubscriptionStatus { Active, Suspended, Cancelled, Expired }
public enum InvoiceStatus      { Draft, Issued, Paid, Overdue, Cancelled }
public enum PaymentMethod      { BankTransfer, CreditCard, Cash, Online }

/// <summary>
/// Represents a subscription plan for a school, including its name, price, maximum number of students and teachers, and available features.
/// </summary>

public class SubscriptionPlan : Entity
{
    public string  Name               { get; private set; } = default!;
    public decimal PriceMonthly       { get; private set; }
    public int     MaxStudents        { get; private set; }
    public int     MaxTeachers        { get; private set; }
    public bool    HasParentPortal    { get; private set; }
    public bool    HasExamModule      { get; private set; }
    public bool    HasAnalytics       { get; private set; }
    public bool    IsActive           { get; private set; }

    private SubscriptionPlan() { }

    private SubscriptionPlan(Guid id, string name, decimal price, int maxStudents,
        int maxTeachers, bool parentPortal, bool examModule, bool analytics) : base(id)
    {
        Name            = name;
        PriceMonthly    = price;
        MaxStudents     = maxStudents;
        MaxTeachers     = maxTeachers;
        HasParentPortal = parentPortal;
        HasExamModule   = examModule;
        HasAnalytics    = analytics;
        IsActive        = true;
    }

    public static SubscriptionPlan Create(string name, decimal price, int maxStudents,
        int maxTeachers, bool parentPortal = true, bool examModule = true, bool analytics = false)
        => new(Guid.NewGuid(), name, price, maxStudents, maxTeachers, parentPortal, examModule, analytics);

    public void Deactivate() => IsActive = false;
}