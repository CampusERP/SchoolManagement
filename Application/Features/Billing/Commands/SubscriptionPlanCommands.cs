using Application.Common.Models;
using MediatR;

namespace Application.Features.Billing.Commands;

public record SubscriptionPlanBenefitInput(
    string? DescriptionEn = null,
    string? DescriptionAr = null);

public record CreateSubscriptionPlanCommand(
    string Name, decimal PriceMonthly, int MaxStudents, int MaxTeachers,
    bool HasParentPortal, bool HasExamModule, bool HasAnalytics,
    string? DescriptionEn = null, string? DescriptionAr = null,
    List<SubscriptionPlanBenefitInput>? Benefits = null)
    : ICommand<Guid>;

public record UpdateSubscriptionPlanCommand(
    Guid PlanId, string Name, decimal PriceMonthly, int MaxStudents, int MaxTeachers,
    bool HasParentPortal, bool HasExamModule, bool HasAnalytics,
    string? DescriptionEn = null, string? DescriptionAr = null,
    List<SubscriptionPlanBenefitInput>? Benefits = null, bool IsActive = true)
    : ICommand;
