using Domain.Common;
using Domain.Exceptions;

namespace Domain.Entities.Billing;

/// <summary>
/// A single selling point shown under a subscription plan. Descriptions are
/// stored in both supported languages and selected by the client.
/// </summary>
public class SubscriptionPlanBenefit : Entity
{
    public Guid SubscriptionPlanId { get; private set; }
    public string? DescriptionEn { get; private set; }
    public string? DescriptionAr { get; private set; }
    public int SortOrder { get; private set; }

    private SubscriptionPlanBenefit() { }

    internal SubscriptionPlanBenefit(
        Guid subscriptionPlanId,
        string? descriptionEn,
        string? descriptionAr,
        int sortOrder)
    {
        SubscriptionPlanId = subscriptionPlanId;
        SetDescriptions(descriptionEn, descriptionAr);
        SortOrder = sortOrder;
    }

    internal void Update(string? descriptionEn, string? descriptionAr, int sortOrder)
    {
        SetDescriptions(descriptionEn, descriptionAr);
        SortOrder = sortOrder;
    }

    private void SetDescriptions(string? descriptionEn, string? descriptionAr)
    {
        DescriptionEn = Normalize(descriptionEn);
        DescriptionAr = Normalize(descriptionAr);

        if (DescriptionEn is null && DescriptionAr is null)
            throw new DomainException("A benefit needs an English or Arabic description.");
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
