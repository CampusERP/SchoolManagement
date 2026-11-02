using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Application.Common.Services;
using Domain.Entities.Billing;
using Domain.Entities.People;
using Domain.Entities.Tenancy;
using MediatR;

namespace Application.Features.SchoolRegistrationRequests.Commands.ApproveSchoolRegistrationRequest;

public class ApproveSchoolRegistrationRequestCommandHandler
    : IRequestHandler<ApproveSchoolRegistrationRequestCommand, Result<Guid>>
{
    private readonly ISchoolRegistrationRequestRepository _requests;
    private readonly ISchoolRepository _schools;
    private readonly ISubscriptionPlanRepository _plans;
    private readonly ISubscriptionRepository _subscriptions;
    private readonly IIdentityService _identity;
    private readonly IUserSchoolMembershipRepository _memberships;
    private readonly ISchoolAdminProfileRepository _adminProfiles;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailNotifier _emailNotifier;

    public ApproveSchoolRegistrationRequestCommandHandler(
        ISchoolRegistrationRequestRepository requests,
        ISchoolRepository schools,
        ISubscriptionPlanRepository plans,
        ISubscriptionRepository subscriptions,
        IIdentityService identity,
        IUserSchoolMembershipRepository memberships,
        ISchoolAdminProfileRepository adminProfiles,
        ICurrentUserService currentUser,
        IEmailNotifier emailNotifier)
    {
        _requests = requests;
        _schools = schools;
        _plans = plans;
        _subscriptions = subscriptions;
        _identity = identity;
        _memberships = memberships;
        _adminProfiles = adminProfiles;
        _currentUser = currentUser;
        _emailNotifier = emailNotifier;
    }

    public async Task<Result<Guid>> Handle(
        ApproveSchoolRegistrationRequestCommand request, CancellationToken ct)
    {
        var entity = await _requests.GetByIdAsync(request.RequestId, ct);
        if (entity is null)
            return Result.Failure<Guid>("Registration request not found.");

        if (entity.Status != SchoolRegistrationRequestStatus.Pending)
            return Result.Failure<Guid>($"Only pending requests can be approved (current status: {entity.Status}).");

        var reviewerId = _currentUser.UserId
            ?? throw new ForbiddenException("Unable to determine the reviewing user.");

        // The desired subdomain must still be free.
        var existing = await _schools.GetBySubdomainAsync(entity.SubdomainCode, ct);
        if (existing is not null)
            return Result.Failure<Guid>($"The subdomain '{entity.SubdomainCode}' is already taken.");

        // 1. Create the school and fill in the contact details.
        var school = School.Create(entity.SchoolName, entity.SubdomainCode);
        school.Update(entity.SchoolName, entity.Address, entity.Phone, entity.SchoolEmail);
        await _schools.AddAsync(school, ct);

        // 2. Create the school-admin login.
        var createResult = await _identity.CreateUserAsync(entity.AdminEmail, entity.AdminPassword, ct);
        if (!createResult.IsSuccess)
            return Result.Failure<Guid>(createResult.Error!);

        var adminUserId = createResult.Value;

        try
        {
            var roleResult = await _identity.AddToRoleAsync(adminUserId, "SchoolAdmin", ct);
            if (!roleResult.IsSuccess)
            {
                await _identity.DeleteUserAsync(adminUserId, ct);
                return Result.Failure<Guid>(roleResult.Error!);
            }

            var membership = UserSchoolMembership.Create(adminUserId, school.Id, "SchoolAdmin");
            await _memberships.AddAsync(membership, ct);

            // The profile lives in the tenant database. It is created directly here
            // (rather than via the outbox) so the admin account is usable immediately.
            var profile = SchoolAdminProfile.Create(
                school.Id, adminUserId, entity.AdminFirstName, entity.AdminLastName);
            await _adminProfiles.AddAsync(profile, ct);
        }
        catch
        {
            // Compensate the Identity user if anything downstream fails.
            await _identity.DeleteUserAsync(adminUserId, ct);
            throw;
        }

        // 3. Provision the subscription the school chose while registering and
        //    issue its first invoice. The paid period only starts once the invoice
        //    is settled by the platform team.
        var planName = entity.PlanName ?? "None";
        var invoiceBlock = string.Empty;
        byte[]? invoicePdf = null;

        if (entity.SubscriptionPlanId.HasValue)
        {
            var plan = await _plans.GetByIdAsync(entity.SubscriptionPlanId.Value, ct);
            if (plan is not null)
            {
                planName = entity.PlanName ?? plan.Name;
                var now = DateTime.UtcNow;
                var amount = entity.PlanPriceMonthly ?? plan.PriceMonthly;

                if (amount <= 0)
                {
                    // Free plans need no invoice, so their period can start right away.
                    _subscriptions.Add(Subscription.Create(
                        school.Id, plan.Id, now, now.AddMonths(1)));
                }
                else
                {
                    var subscription = Subscription.CreatePendingPayment(school.Id, plan.Id, now);
                    var invoice = subscription.GenerateInvoice(
                        amount,
                        now.AddDays(7),
                        $"First invoice — {planName} plan");
                    invoicePdf = InvoicePdfGenerator.Create(
                        entity.SchoolName, planName, invoice.Id.ToString(), amount, invoice.DueDate);
                    _subscriptions.Add(subscription);

                    invoiceBlock =
                        "<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" " +
                        "style=\"margin:0 0 20px;background:#eff6ff;border:1px solid #bfdbfe;border-radius:8px;\">" +
                        "<tr><td style=\"padding:16px;\">" +
                        "<p style=\"margin:0 0 8px;\"><strong>First invoice</strong></p>" +
                        $"<p style=\"margin:0 0 8px;\">Amount: {amount:0.00}</p>" +
                        $"<p style=\"margin:0 0 8px;\">Due date: {invoice.DueDate:yyyy-MM-dd}</p>" +
                        $"<p style=\"margin:0;\">Reference: {invoice.Id}</p>" +
                        "</td></tr></table>" +
                        "<p style=\"margin:0 0 20px;\">Contact our team to arrange payment. " +
                        "Your subscription period starts automatically once the invoice is paid in full.</p>";
                }
            }
        }

        // 4. Mark the request as approved and link the created records.
        entity.Approve(school.Id, adminUserId, reviewerId, request.Notes);

        // 5. Let the new school administrator know they can sign in.
        var approvalEmail = new Dictionary<string, string?>
            {
                ["ContactName"] = $"{entity.AdminFirstName} {entity.AdminLastName}".Trim(),
                ["SchoolName"] = entity.SchoolName,
                ["Email"] = entity.AdminEmail,
                ["PlanName"] = planName,
                ["LoginUrl"] = $"{_emailNotifier.AppBaseUrl}/auth/login",
                ["InvoiceBlock"] = invoiceBlock
            };
        if (invoicePdf is not null)
            await _emailNotifier.SendToEmailWithAttachmentAsync(
                entity.AdminEmail, EmailTemplate.SchoolRegistrationApproved, approvalEmail,
                "school-invoice.pdf", invoicePdf, ct);
        else
            await _emailNotifier.SendToEmailAsync(
                entity.AdminEmail, EmailTemplate.SchoolRegistrationApproved, approvalEmail, ct);

        return Result.Success(school.Id);
    }
}
