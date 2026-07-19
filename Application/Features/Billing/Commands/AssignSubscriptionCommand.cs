using Application.Common.Models;
using MediatR;

namespace Application.Features.Billing.Commands;

public record AssignSubscriptionCommand(
    Guid SchoolId, Guid SubscriptionPlanId, DateTime StartDate, DateTime EndDate)
    : ICommand<Guid>;
