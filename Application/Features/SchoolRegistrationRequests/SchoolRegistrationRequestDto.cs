using Domain.Entities.Tenancy;

namespace Application.Features.SchoolRegistrationRequests;

/// <summary>
/// A read model for a school registration request. Note that the admin password is
/// intentionally never included here.
/// </summary>
public record SchoolRegistrationRequestDto(
    Guid Id,
    string SchoolName,
    string SubdomainCode,
    string? Address,
    string? Phone,
    string? SchoolEmail,
    string AdminFirstName,
    string AdminLastName,
    string AdminEmail,
    Guid? SubscriptionPlanId,
    string? PlanName,
    decimal? PlanPriceMonthly,
    string Status,
    string? ReviewNotes,
    Guid? ReviewedByUserId,
    DateTime? ReviewedAtUtc,
    Guid? CreatedSchoolId,
    Guid? CreatedAdminUserId,
    DateTime CreatedAtUtc)
{
    public static SchoolRegistrationRequestDto FromEntity(SchoolRegistrationRequest r) =>
        new(
            r.Id,
            r.SchoolName,
            r.SubdomainCode,
            r.Address,
            r.Phone,
            r.SchoolEmail,
            r.AdminFirstName,
            r.AdminLastName,
            r.AdminEmail,
            r.SubscriptionPlanId,
            r.PlanName,
            r.PlanPriceMonthly,
            r.Status,
            r.ReviewNotes,
            r.ReviewedByUserId,
            r.ReviewedAtUtc,
            r.CreatedSchoolId,
            r.CreatedAdminUserId,
            r.CreatedAtUtc);
}
