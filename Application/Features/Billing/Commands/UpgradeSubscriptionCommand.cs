using Application.Common.Models;
using MediatR;

namespace Application.Features.Billing.Commands;

public record UpgradeSubscriptionCommand(Guid SchoolId, Guid NewPlanId)
    : ICommand;
