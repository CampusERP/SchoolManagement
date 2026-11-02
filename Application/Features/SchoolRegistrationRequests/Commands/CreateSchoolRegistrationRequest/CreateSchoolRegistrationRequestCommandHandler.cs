using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Domain.Entities.Tenancy;
using MediatR;

namespace Application.Features.SchoolRegistrationRequests.Commands.CreateSchoolRegistrationRequest;

public class CreateSchoolRegistrationRequestCommandHandler
    : IRequestHandler<CreateSchoolRegistrationRequestCommand, Result<Guid>>
{
    private readonly ISchoolRegistrationRequestRepository _requests;
    private readonly ISchoolRepository _schools;
    private readonly ISubscriptionPlanRepository _plans;
    private readonly IEmailNotifier _emailNotifier;

    public CreateSchoolRegistrationRequestCommandHandler(
        ISchoolRegistrationRequestRepository requests,
        ISchoolRepository schools,
        ISubscriptionPlanRepository plans,
        IEmailNotifier emailNotifier)
    {
        _requests = requests;
        _schools = schools;
        _plans = plans;
        _emailNotifier = emailNotifier;
    }

    public async Task<Result<Guid>> Handle(
        CreateSchoolRegistrationRequestCommand request, CancellationToken ct)
    {
        var subdomain = request.SubdomainCode.Trim().ToLowerInvariant();

        // The desired subdomain must not already belong to a live school.
        var existingSchool = await _schools.GetBySubdomainAsync(subdomain, ct);
        if (existingSchool is not null)
            return Result.Failure<Guid>($"The subdomain '{subdomain}' is already taken.");

        // Avoid piling up duplicate pending requests for the same subdomain.
        var pending = await _requests.GetPendingBySubdomainAsync(subdomain, ct);
        if (pending is not null)
            return Result.Failure<Guid>(
                "A pending registration request for this subdomain already exists.");

        // Snapshot the chosen plan so the first invoice uses the price the school
        // agreed to, even if the plan is edited before the request is approved.
        Guid? planId = null;
        string? planName = null;
        decimal? planPrice = null;

        if (request.SubscriptionPlanId.HasValue)
        {
            var plan = await _plans.GetByIdAsync(request.SubscriptionPlanId.Value, ct);
            if (plan is null || !plan.IsActive)
                return Result.Failure<Guid>("The selected subscription plan is not available.");

            planId = plan.Id;
            planName = plan.Name;
            planPrice = plan.PriceMonthly;
        }

        var entity = SchoolRegistrationRequest.Create(
            request.SchoolName,
            subdomain,
            request.Address,
            request.Phone,
            request.SchoolEmail,
            request.AdminFirstName,
            request.AdminLastName,
            request.AdminEmail,
            request.AdminPassword,
            planId,
            planName,
            planPrice);

        await _requests.AddAsync(entity, ct);

        await _emailNotifier.SendToEmailAsync(
            entity.AdminEmail,
            EmailTemplate.SchoolRegistrationReceived,
            new Dictionary<string, string?>
            {
                ["ContactName"] = $"{entity.AdminFirstName} {entity.AdminLastName}".Trim(),
                ["SchoolName"] = entity.SchoolName,
                ["Subdomain"] = entity.SubdomainCode,
                ["PlanName"] = entity.PlanName ?? "To be selected with the platform team",
                ["RequestId"] = entity.Id.ToString()
            },
            ct);

        await _emailNotifier.NotifyPlatformAdminsAsync(
            EmailTemplate.Notification,
            new Dictionary<string, string?>
            {
                ["Body"] = System.Net.WebUtility.HtmlEncode($"New school registration request: {entity.SchoolName} ({entity.SubdomainCode}). " +
                    $"Administrator: {entity.AdminFirstName} {entity.AdminLastName} <{entity.AdminEmail}>. " +
                    $"Request reference: {entity.Id}. Review it in the platform portal.")
            },
            ct);

        return Result.Success(entity.Id);
    }
}
