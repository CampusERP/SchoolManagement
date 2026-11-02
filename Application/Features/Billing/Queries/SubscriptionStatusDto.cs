namespace Application.Features.Billing.Queries;

public record SubscriptionStatusDto(
    Guid SubscriptionId,
    Guid SchoolId,
    string SchoolName,
    Guid PlanId,
    string PlanName,
    string? PlanDescriptionEn,
    string? PlanDescriptionAr,
    IReadOnlyList<SubscriptionPlanBenefitDto> PlanBenefits,
    decimal PriceMonthly,
    string Status,
    DateTime StartDate,
    DateTime EndDate,
    bool AutoRenew,
    int MaxStudents,
    int MaxTeachers,
    bool HasParentPortal,
    bool HasExamModule,
    bool HasAnalytics,
    int DaysRemaining,
    bool IsActive,
    decimal OutstandingBalance);
