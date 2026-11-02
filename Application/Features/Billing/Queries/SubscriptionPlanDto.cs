namespace Application.Features.Billing.Queries;

public record SubscriptionPlanBenefitDto(
    Guid Id,
    string? DescriptionEn,
    string? DescriptionAr,
    int SortOrder);

public record SubscriptionPlanDto(
    Guid Id,
    string Name,
    string? DescriptionEn,
    string? DescriptionAr,
    IReadOnlyList<SubscriptionPlanBenefitDto> Benefits,
    decimal PriceMonthly,
    int MaxStudents,
    int MaxTeachers,
    bool HasParentPortal,
    bool HasExamModule,
    bool HasAnalytics,
    bool IsActive);
