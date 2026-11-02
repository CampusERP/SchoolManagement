using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Entities.Tenancy;
using MediatR;

namespace Application.Features.SchoolRegistrationRequests.Commands.UpdateSchoolRegistrationRequest;

public class UpdateSchoolRegistrationRequestCommandHandler
    : IRequestHandler<UpdateSchoolRegistrationRequestCommand, Result>
{
    private readonly ISchoolRegistrationRequestRepository _requests;
    private readonly ISchoolRepository _schools;
    private readonly ISubscriptionPlanRepository _plans;

    public UpdateSchoolRegistrationRequestCommandHandler(
        ISchoolRegistrationRequestRepository requests,
        ISchoolRepository schools,
        ISubscriptionPlanRepository plans)
    {
        _requests = requests;
        _schools = schools;
        _plans = plans;
    }

    public async Task<Result> Handle(
        UpdateSchoolRegistrationRequestCommand request, CancellationToken ct)
    {
        var entity = await _requests.GetByIdAsync(request.RequestId, ct);
        if (entity is null)
            return Result.Failure("Registration request not found.");

        if (entity.Status != SchoolRegistrationRequestStatus.Pending)
            return Result.Failure("Only pending requests can be edited.");

        var subdomain = request.SubdomainCode.Trim().ToLowerInvariant();

        // Guard against colliding with a live school or another pending request.
        var existingSchool = await _schools.GetBySubdomainAsync(subdomain, ct);
        if (existingSchool is not null)
            return Result.Failure($"The subdomain '{subdomain}' is already taken.");

        if (!string.Equals(subdomain, entity.SubdomainCode, StringComparison.OrdinalIgnoreCase))
        {
            var pending = await _requests.GetPendingBySubdomainAsync(subdomain, ct);
            if (pending is not null)
                return Result.Failure("A pending registration request for this subdomain already exists.");
        }

        Guid? planId = null;
        string? planName = null;
        decimal? planPrice = null;

        if (request.SubscriptionPlanId.HasValue)
        {
            var plan = await _plans.GetByIdAsync(request.SubscriptionPlanId.Value, ct);
            if (plan is null || !plan.IsActive)
                return Result.Failure("The selected subscription plan is not available.");

            planId = plan.Id;
            planName = plan.Name;
            planPrice = plan.PriceMonthly;
        }

        entity.Update(
            request.SchoolName,
            subdomain,
            request.Address,
            request.Phone,
            request.SchoolEmail,
            request.AdminFirstName,
            request.AdminLastName,
            request.AdminEmail,
            request.AdminPassword,
            request.ReviewNotes,
            planId,
            planName,
            planPrice);

        return Result.Success();
    }
}
