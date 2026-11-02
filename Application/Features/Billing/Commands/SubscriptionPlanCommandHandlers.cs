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

    public Task<Result<Guid>> Handle(CreateSubscriptionPlanCommand request, CancellationToken ct)
    {
        var plan = SubscriptionPlan.Create(
            request.Name, request.PriceMonthly, request.MaxStudents,
            request.MaxTeachers, request.HasParentPortal,
            request.HasExamModule, request.HasAnalytics,
            request.DescriptionEn, request.DescriptionAr);

        if (request.Benefits is { Count: > 0 })
            plan.ReplaceBenefits(ToBenefitValues(request.Benefits));

        _repository.Add(plan);
        return Task.FromResult(Result.Success(plan.Id));
    }

    internal static IEnumerable<(string? DescriptionEn, string? DescriptionAr)> ToBenefitValues(
        IEnumerable<SubscriptionPlanBenefitInput> benefits) =>
        benefits.Select(b => (b.DescriptionEn, b.DescriptionAr));
}

public class UpdateSubscriptionPlanCommandHandler
    : IRequestHandler<UpdateSubscriptionPlanCommand, Result>
{
    private readonly ISubscriptionPlanRepository _repository;
    public UpdateSubscriptionPlanCommandHandler(ISubscriptionPlanRepository repository) => _repository = repository;

    public async Task<Result> Handle(UpdateSubscriptionPlanCommand request, CancellationToken ct)
    {
        var plan = await _repository.GetByIdAsync(request.PlanId, ct);
        if (plan is null)
            return Result.Failure("Subscription plan not found.");

        try
        {
            var benefits = request.Benefits is null
                ? null
                : CreateSubscriptionPlanCommandHandler.ToBenefitValues(request.Benefits);

            plan.Update(request.Name, request.PriceMonthly, request.MaxStudents, request.MaxTeachers,
                request.HasParentPortal, request.HasExamModule, request.HasAnalytics,
                request.DescriptionEn, request.DescriptionAr, benefits);

            if (request.IsActive) plan.Activate();
            else plan.Deactivate();

            _repository.Update(plan);
            return Result.Success();
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
