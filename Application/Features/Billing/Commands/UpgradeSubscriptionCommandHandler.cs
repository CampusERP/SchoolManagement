using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Billing.Commands;

public class UpgradeSubscriptionCommandHandler
    : IRequestHandler<UpgradeSubscriptionCommand, Result>
{
    private readonly ISubscriptionRepository _repository;
    public UpgradeSubscriptionCommandHandler(ISubscriptionRepository repository) => _repository = repository;

    public async Task<Result> Handle(UpgradeSubscriptionCommand request, CancellationToken ct)
    {
        var sub = await _repository.GetActiveBySchoolIdAsync(request.SchoolId, ct);
        if (sub is null)
            return Result.Failure("No active subscription found.");

        sub.Upgrade(request.NewPlanId);
        _repository.Update(sub);
        return Result.Success();
    }
}
