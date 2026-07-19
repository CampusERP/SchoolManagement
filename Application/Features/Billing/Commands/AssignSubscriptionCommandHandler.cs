using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Entities.Billing;
using MediatR;

namespace Application.Features.Billing.Commands;

public class AssignSubscriptionCommandHandler
    : IRequestHandler<AssignSubscriptionCommand, Result<Guid>>
{
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;

    public AssignSubscriptionCommandHandler(
        ISubscriptionPlanRepository planRepository,
        ISubscriptionRepository subscriptionRepository)
    {
        _planRepository = planRepository;
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<Result<Guid>> Handle(AssignSubscriptionCommand request, CancellationToken ct)
    {
        var plan = await _planRepository.GetByIdAsync(request.SubscriptionPlanId, ct);
        if (plan is null || !plan.IsActive)
            return Result.Failure<Guid>("Subscription plan not found or inactive.");

        // Cancel existing active subscription if any
        var existing = await _subscriptionRepository.GetActiveBySchoolIdAsync(request.SchoolId, ct);
        if (existing is not null)
        {
            existing.Cancel();
            _subscriptionRepository.Update(existing);
        }

        var sub = Subscription.Create(request.SchoolId, request.SubscriptionPlanId,
            request.StartDate, request.EndDate);
        _subscriptionRepository.Add(sub);
        return Result.Success(sub.Id);
    }
}
