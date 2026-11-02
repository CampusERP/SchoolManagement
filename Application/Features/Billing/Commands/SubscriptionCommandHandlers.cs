using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Domain.Entities.Billing;
using MediatR;

namespace Application.Features.Billing.Commands;

public class AssignSubscriptionCommandHandler
    : IRequestHandler<AssignSubscriptionCommand, Result<Guid>>
{
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ISchoolRepository _schools;
    private readonly IEmailNotifier _emailNotifier;

    public AssignSubscriptionCommandHandler(
        ISubscriptionPlanRepository planRepository,
        ISubscriptionRepository subscriptionRepository,
        ISchoolRepository schools,
        IEmailNotifier emailNotifier)
    {
        _planRepository = planRepository;
        _subscriptionRepository = subscriptionRepository;
        _schools = schools;
        _emailNotifier = emailNotifier;
    }

    public async Task<Result<Guid>> Handle(AssignSubscriptionCommand request, CancellationToken ct)
    {
        var plan = await _planRepository.GetByIdAsync(request.SubscriptionPlanId, ct);
        if (plan is null || !plan.IsActive)
            return Result.Failure<Guid>("Subscription plan not found or inactive.");

        var now = DateTime.UtcNow;
        var start = request.StartDate?.ToUniversalTime() ?? now;
        var end = request.EndDate?.ToUniversalTime() ?? start.AddMonths(1);

        if (end <= start)
            return Result.Failure<Guid>("End date must be after the start date.");

        // Re-use the current subscription instead of creating a second one;
        // the database only allows a single active subscription per school.
        var existing = await _subscriptionRepository.GetLatestBySchoolIdAsync(request.SchoolId, ct);
        if (existing is not null && existing.Status != SubscriptionStatus.Cancelled)
        {
            try
            {
                existing.ChangePlan(request.SubscriptionPlanId, start, end);
            }
            catch (Domain.Exceptions.DomainException ex)
            {
                return Result.Failure<Guid>(ex.Message);
            }

            _subscriptionRepository.Update(existing);
            await NotifySchoolAsync(request.SchoolId, plan.Name, existing.Status.ToString(), ct);
            return Result.Success(existing.Id);
        }

        var subscription = Subscription.Create(request.SchoolId, request.SubscriptionPlanId, start, end);
        _subscriptionRepository.Add(subscription);
        await NotifySchoolAsync(request.SchoolId, plan.Name, subscription.Status.ToString(), ct);
        return Result.Success(subscription.Id);
    }

    private async Task NotifySchoolAsync(Guid schoolId, string planName, string status, CancellationToken ct)
    {
        var school = await _schools.GetByIdAsync(schoolId, ct);
        await _emailNotifier.SendToSchoolAdminsAsync(schoolId, EmailTemplate.Notification,
            new Dictionary<string, string?>
            {
                ["Body"] = System.Net.WebUtility.HtmlEncode($"The subscription for {school?.Name ?? "your school"} was updated. " +
                    $"Plan: {planName}. Status: {status}.")
            }, ct);
    }
}

public class UpgradeSubscriptionCommandHandler
    : IRequestHandler<UpgradeSubscriptionCommand, Result>
{
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ISchoolRepository _schools;
    private readonly IEmailNotifier _emailNotifier;

    public UpgradeSubscriptionCommandHandler(
        ISubscriptionPlanRepository planRepository,
        ISubscriptionRepository subscriptionRepository,
        ISchoolRepository schools,
        IEmailNotifier emailNotifier)
    {
        _planRepository = planRepository;
        _subscriptionRepository = subscriptionRepository;
        _schools = schools;
        _emailNotifier = emailNotifier;
    }

    public async Task<Result> Handle(UpgradeSubscriptionCommand request, CancellationToken ct)
    {
        var plan = await _planRepository.GetByIdAsync(request.NewPlanId, ct);
        if (plan is null || !plan.IsActive)
            return Result.Failure("Subscription plan not found or inactive.");

        var subscription = await _subscriptionRepository.GetLatestBySchoolIdAsync(request.SchoolId, ct);
        if (subscription is null)
            return Result.Failure("No subscription found for this school.");

        try
        {
            subscription.Upgrade(request.NewPlanId);
            _subscriptionRepository.Update(subscription);
            var school = await _schools.GetByIdAsync(request.SchoolId, ct);
            await _emailNotifier.SendToSchoolAdminsAsync(request.SchoolId, EmailTemplate.Notification,
                new Dictionary<string, string?>
                {
                    ["Body"] = System.Net.WebUtility.HtmlEncode($"The subscription plan for {school?.Name ?? "your school"} has changed to {plan.Name}. " +
                        $"Current status: {subscription.Status}.")
                }, ct);
            return Result.Success();
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}

public class SuspendSubscriptionCommandHandler : IRequestHandler<SuspendSubscriptionCommand, Result>
{
    private readonly ISubscriptionRepository _repository;
    private readonly ISchoolRepository _schools;
    private readonly ISubscriptionPlanRepository _plans;
    private readonly IEmailNotifier _emailNotifier;

    public SuspendSubscriptionCommandHandler(
        ISubscriptionRepository repository,
        ISchoolRepository schools,
        ISubscriptionPlanRepository plans,
        IEmailNotifier emailNotifier)
    {
        _repository = repository;
        _schools = schools;
        _plans = plans;
        _emailNotifier = emailNotifier;
    }

    public async Task<Result> Handle(SuspendSubscriptionCommand request, CancellationToken ct)
    {
        var subscription = await _repository.GetLatestBySchoolIdAsync(request.SchoolId, ct);
        if (subscription is null) return Result.Failure("No subscription found for this school.");

        try
        {
            subscription.Suspend();
            _repository.Update(subscription);

            var school = await _schools.GetByIdAsync(request.SchoolId, ct);
            var plan = await _plans.GetByIdAsync(subscription.SubscriptionPlanId, ct);
            await _emailNotifier.SendToSchoolAdminsAsync(
                request.SchoolId,
                EmailTemplate.SubscriptionSuspended,
                new Dictionary<string, string?>
                {
                    ["ContactName"] = "School Administrator",
                    ["SchoolName"] = school?.Name ?? "School",
                    ["PlanName"] = plan?.Name ?? "Subscription",
                    ["EndDate"] = subscription.EndDate.ToString("yyyy-MM-dd")
                },
                ct);

            return Result.Success();
        }
        catch (Domain.Exceptions.DomainException ex) { return Result.Failure(ex.Message); }
    }
}

public class ReactivateSubscriptionCommandHandler : IRequestHandler<ReactivateSubscriptionCommand, Result>
{
    private readonly ISubscriptionRepository _repository;
    private readonly ISchoolRepository _schools;
    private readonly ISubscriptionPlanRepository _plans;
    private readonly IEmailNotifier _emailNotifier;

    public ReactivateSubscriptionCommandHandler(
        ISubscriptionRepository repository,
        ISchoolRepository schools,
        ISubscriptionPlanRepository plans,
        IEmailNotifier emailNotifier)
    {
        _repository = repository;
        _schools = schools;
        _plans = plans;
        _emailNotifier = emailNotifier;
    }

    public async Task<Result> Handle(ReactivateSubscriptionCommand request, CancellationToken ct)
    {
        var subscription = await _repository.GetLatestBySchoolIdAsync(request.SchoolId, ct);
        if (subscription is null) return Result.Failure("No subscription found for this school.");

        try
        {
            subscription.Reactivate(DateTime.UtcNow);
            _repository.Update(subscription);

            var school = await _schools.GetByIdAsync(request.SchoolId, ct);
            var plan = await _plans.GetByIdAsync(subscription.SubscriptionPlanId, ct);
            await _emailNotifier.SendToSchoolAdminsAsync(
                request.SchoolId,
                EmailTemplate.SubscriptionReactivated,
                new Dictionary<string, string?>
                {
                    ["ContactName"] = "School Administrator",
                    ["SchoolName"] = school?.Name ?? "School",
                    ["PlanName"] = plan?.Name ?? "Subscription",
                    ["EndDate"] = subscription.EndDate.ToString("yyyy-MM-dd"),
                    ["LoginUrl"] = $"{_emailNotifier.AppBaseUrl}/auth/login"
                },
                ct);

            return Result.Success();
        }
        catch (Domain.Exceptions.DomainException ex) { return Result.Failure(ex.Message); }
    }
}

public class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand, Result>
{
    private readonly ISubscriptionRepository _repository;
    private readonly ISchoolRepository _schools;
    private readonly ISubscriptionPlanRepository _plans;
    private readonly IEmailNotifier _emailNotifier;

    public CancelSubscriptionCommandHandler(
        ISubscriptionRepository repository,
        ISchoolRepository schools,
        ISubscriptionPlanRepository plans,
        IEmailNotifier emailNotifier)
    {
        _repository = repository;
        _schools = schools;
        _plans = plans;
        _emailNotifier = emailNotifier;
    }

    public async Task<Result> Handle(CancelSubscriptionCommand request, CancellationToken ct)
    {
        var subscription = await _repository.GetLatestBySchoolIdAsync(request.SchoolId, ct);
        if (subscription is null) return Result.Failure("No subscription found for this school.");

        subscription.Cancel();
        _repository.Update(subscription);

        var school = await _schools.GetByIdAsync(request.SchoolId, ct);
        var plan = await _plans.GetByIdAsync(subscription.SubscriptionPlanId, ct);
        await _emailNotifier.SendToSchoolAdminsAsync(
            request.SchoolId,
            EmailTemplate.SubscriptionCancelled,
            new Dictionary<string, string?>
            {
                ["ContactName"] = "School Administrator",
                ["SchoolName"] = school?.Name ?? "School",
                ["PlanName"] = plan?.Name ?? "Subscription",
                ["EndDate"] = subscription.EndDate.ToString("yyyy-MM-dd")
            },
            ct);

        return Result.Success();
    }
}
