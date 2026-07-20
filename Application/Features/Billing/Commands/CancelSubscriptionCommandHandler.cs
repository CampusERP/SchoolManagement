using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Billing.Commands;

public class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand, Result>
{
    private readonly ISubscriptionRepository _repository;
    public CancelSubscriptionCommandHandler(ISubscriptionRepository repository) => _repository = repository;

    public async Task<Result> Handle(CancelSubscriptionCommand request, CancellationToken ct)
    {
        var sub = await _repository.GetActiveBySchoolIdAsync(request.SchoolId, ct);
        if (sub is null) return Result.Failure("No subscription found.");
        sub.Cancel();
        _repository.Update(sub);
        return Result.Success();
    }
}
