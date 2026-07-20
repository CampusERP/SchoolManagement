using Application.Common.Models;
using MediatR;

namespace Application.Features.Billing.Commands;

public record CreateSubscriptionPlanCommand(
    string Name, decimal PriceMonthly, int MaxStudents, int MaxTeachers,
    bool HasParentPortal, bool HasExamModule, bool HasAnalytics)
    : ICommand<Guid>;
