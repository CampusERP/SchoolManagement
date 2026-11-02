using Domain.Common;

namespace Domain.Entities.Billing;

public enum SubscriptionStatus { PendingPayment, Active, Suspended, Cancelled, Expired }
public enum InvoiceStatus      { Draft, Issued, Paid, Overdue, Cancelled }
public enum PaymentMethod      { BankTransfer, CreditCard, Cash, Online }

/// <summary>
/// Represents a subscription plan for a school, including its localized name,
/// descriptions, price, limits, and ordered list of selling points.
/// </summary>

public class SubscriptionPlan : Entity
{
    private readonly List<SubscriptionPlanBenefit> _benefits = new();

    public string  Name               { get; private set; } = default!;
    public string? DescriptionEn      { get; private set; }
    public string? DescriptionAr      { get; private set; }
    public decimal PriceMonthly       { get; private set; }
    public int     MaxStudents        { get; private set; }
    public int     MaxTeachers        { get; private set; }
    public bool    HasParentPortal    { get; private set; }
    public bool    HasExamModule      { get; private set; }
    public bool    HasAnalytics       { get; private set; }
    public bool    IsActive           { get; private set; }

    public IReadOnlyCollection<SubscriptionPlanBenefit> Benefits => _benefits.AsReadOnly();

    private SubscriptionPlan() { }

    private SubscriptionPlan(Guid id, string name, decimal price, int maxStudents,
        int maxTeachers, bool parentPortal, bool examModule, bool analytics,
        string? descriptionEn, string? descriptionAr) : base(id)
    {
        Name            = name;
        DescriptionEn   = Normalize(descriptionEn);
        DescriptionAr   = Normalize(descriptionAr);
        PriceMonthly    = price;
        MaxStudents     = maxStudents;
        MaxTeachers     = maxTeachers;
        HasParentPortal = parentPortal;
        HasExamModule   = examModule;
        HasAnalytics    = analytics;
        IsActive        = true;
    }

    public static SubscriptionPlan Create(string name, decimal price, int maxStudents,
        int maxTeachers, bool parentPortal = true, bool examModule = true, bool analytics = false,
        string? descriptionEn = null, string? descriptionAr = null)
        => new(Guid.NewGuid(), name, price, maxStudents, maxTeachers, parentPortal, examModule,
               analytics, descriptionEn, descriptionAr);

    /// <summary>Edits the plan definition. Kept for platform administration.</summary>
    public void Update(string name, decimal price, int maxStudents, int maxTeachers,
        bool parentPortal, bool examModule, bool analytics, string? descriptionEn,
        string? descriptionAr, IEnumerable<(string? DescriptionEn, string? DescriptionAr)>? benefits = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Domain.Exceptions.DomainException("Plan name is required.");
        if (price < 0)
            throw new Domain.Exceptions.DomainException("Plan price cannot be negative.");

        Name            = name.Trim();
        PriceMonthly    = price;
        MaxStudents     = maxStudents;
        MaxTeachers     = maxTeachers;
        HasParentPortal = parentPortal;
        HasExamModule   = examModule;
        HasAnalytics    = analytics;
        DescriptionEn   = Normalize(descriptionEn);
        DescriptionAr   = Normalize(descriptionAr);

        if (benefits is not null)
            ReplaceBenefits(benefits);
    }

    /// <summary>Replaces the ordered benefit bullets of this plan.</summary>
    public void ReplaceBenefits(IEnumerable<(string? DescriptionEn, string? DescriptionAr)> benefits)
    {
        _benefits.Clear();

        var sortOrder = 0;
        foreach (var benefit in benefits)
        {
            var descriptionEn = Normalize(benefit.DescriptionEn);
            var descriptionAr = Normalize(benefit.DescriptionAr);
            if (descriptionEn is null && descriptionAr is null)
                continue;

            _benefits.Add(new SubscriptionPlanBenefit(Id, descriptionEn, descriptionAr, sortOrder++));
        }
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
