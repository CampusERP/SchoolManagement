using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Billing.Commands;

public class SuspendSubscriptionCommandHandler : IRequestHandler<SuspendSubscriptionCommand, Result>
{
    private readonly ISubscriptionRepository _repository;
    public SuspendSubscriptionCommandHandler(ISubscriptionRepository repository) => _repository = repository;

    public async Task<Result> Handle(SuspendSubscriptionCommand request, CancellationToken ct)
    {
        var sub = await _repository.GetActiveBySchoolIdAsync(request.SchoolId, ct);
        if (sub is null) return Result.Failure("No active subscription found.");
        try
        {
            sub.Suspend();
            _repository.Update(sub);
            return Result.Success();
        }
        catch (Domain.Exceptions.DomainException ex) { return Result.Failure(ex.Message); }
    }
}
