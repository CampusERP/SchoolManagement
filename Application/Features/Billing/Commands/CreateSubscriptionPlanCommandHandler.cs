using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Entities.Billing;
using MediatR;

namespace Application.Features.Billing.Commands;

public class CreateSubscriptionPlanCommandHandler
    : IRequestHandler<CreateSubscriptionPlanCommand, Result<Guid>>
{
    private readonly ISubscriptionPlanRepository _repository;
    public CreateSubscriptionPlanCommandHandler(ISubscriptionPlanRepository repository) => _repository = repository;

    public async Task<Result<Guid>> Handle(CreateSubscriptionPlanCommand request, CancellationToken ct)
    {
        var plan = SubscriptionPlan.Create(
            request.Name, request.PriceMonthly, request.MaxStudents,
            request.MaxTeachers, request.HasParentPortal,
            request.HasExamModule, request.HasAnalytics);

        _repository.Add(plan);
        return Result.Success(plan.Id);
    }
}
