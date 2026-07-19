namespace Application.Features.Billing.Queries;

public record SubscriptionPlanDto(Guid Id, string Name, decimal PriceMonthly,
    int MaxStudents, int MaxTeachers,
    bool HasParentPortal, bool HasExamModule, bool HasAnalytics);
