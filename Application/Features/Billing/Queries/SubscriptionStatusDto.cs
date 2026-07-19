namespace Application.Features.Billing.Queries;

public record SubscriptionStatusDto(Guid SubscriptionId, string PlanName,
    decimal PriceMonthly, string Status, DateTime StartDate, DateTime EndDate,
    bool AutoRenew, int MaxStudents, int MaxTeachers,
    bool HasParentPortal, bool HasExamModule, bool HasAnalytics,
    int DaysRemaining);
