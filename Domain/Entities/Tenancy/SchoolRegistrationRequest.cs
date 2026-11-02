using Domain.Common;
using Domain.Exceptions;

namespace Domain.Entities.Tenancy;

/// <summary>
/// The lifecycle states of a <see cref="SchoolRegistrationRequest"/>.
/// </summary>
public static class SchoolRegistrationRequestStatus
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}

/// <summary>
/// A public "request to join" submitted by a prospective school from the landing page.
/// It captures the school details plus the first school-admin credentials so a platform
/// super admin can review the request and, on approval, provision the <see cref="School"/>
/// and the school admin account.
///
/// This entity lives in the platform database (PlatformDbContext) and is intentionally
/// isolated from tenant-scoped data: it belongs to the platform, not to any school.
/// </summary>
public class SchoolRegistrationRequest : AuditableEntity, IAggregateRoot
{
    public string SchoolName { get; private set; } = default!;
    public string SubdomainCode { get; private set; } = default!;
    public string? Address { get; private set; }
    public string? Phone { get; private set; }
    public string? SchoolEmail { get; private set; }

    public string AdminFirstName { get; private set; } = default!;
    public string AdminLastName { get; private set; } = default!;
    public string AdminEmail { get; private set; } = default!;

    /// <summary>
    /// The school-admin password chosen at request time. It is stored so the platform
    /// super admin can provision the account on approval, and is NEVER returned by the API.
    /// </summary>
    public string AdminPassword { get; private set; } = default!;

    /// <summary>
    /// The subscription plan chosen by the school at registration time. The plan name
    /// and price are snapshotted so later plan edits cannot change the first invoice.
    /// </summary>
    public Guid? SubscriptionPlanId { get; private set; }
    public string? PlanName { get; private set; }
    public decimal? PlanPriceMonthly { get; private set; }

    public string Status { get; private set; } = SchoolRegistrationRequestStatus.Pending;
    public string? ReviewNotes { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public DateTime? ReviewedAtUtc { get; private set; }

    /// <summary>The school created when this request was approved.</summary>
    public Guid? CreatedSchoolId { get; private set; }

    /// <summary>The school-admin user created when this request was approved.</summary>
    public Guid? CreatedAdminUserId { get; private set; }

    private SchoolRegistrationRequest() { } // EF Core

    private SchoolRegistrationRequest(
        Guid id,
        string schoolName,
        string subdomainCode,
        string? address,
        string? phone,
        string? schoolEmail,
        string adminFirstName,
        string adminLastName,
        string adminEmail,
        string adminPassword,
        Guid? subscriptionPlanId = null,
        string? planName = null,
        decimal? planPriceMonthly = null) : base(id)
    {
        SchoolName = schoolName;
        SubdomainCode = subdomainCode.ToLowerInvariant();
        Address = address;
        Phone = phone;
        SchoolEmail = schoolEmail;
        AdminFirstName = adminFirstName;
        AdminLastName = adminLastName;
        AdminEmail = adminEmail;
        AdminPassword = adminPassword;
        SubscriptionPlanId = subscriptionPlanId;
        PlanName = planName;
        PlanPriceMonthly = planPriceMonthly;
        Status = SchoolRegistrationRequestStatus.Pending;
    }

    public static SchoolRegistrationRequest Create(
        string schoolName,
        string subdomainCode,
        string? address,
        string? phone,
        string? schoolEmail,
        string adminFirstName,
        string adminLastName,
        string adminEmail,
        string adminPassword,
        Guid? subscriptionPlanId = null,
        string? planName = null,
        decimal? planPriceMonthly = null)
    {
        if (string.IsNullOrWhiteSpace(schoolName))
            throw new DomainException("School name is required.");
        if (string.IsNullOrWhiteSpace(subdomainCode))
            throw new DomainException("Subdomain code is required.");
        if (string.IsNullOrWhiteSpace(adminFirstName) || string.IsNullOrWhiteSpace(adminLastName))
            throw new DomainException("School admin first and last name are required.");
        if (string.IsNullOrWhiteSpace(adminEmail))
            throw new DomainException("School admin email is required.");
        if (string.IsNullOrWhiteSpace(adminPassword))
            throw new DomainException("School admin password is required.");

        return new SchoolRegistrationRequest(
            Guid.NewGuid(),
            schoolName.Trim(),
            subdomainCode.Trim().ToLowerInvariant(),
            address?.Trim(),
            phone?.Trim(),
            schoolEmail?.Trim(),
            adminFirstName.Trim(),
            adminLastName.Trim(),
            adminEmail.Trim().ToLowerInvariant(),
            adminPassword,
            subscriptionPlanId,
            planName,
            planPriceMonthly);
    }

    /// <summary>Edit the captured details. Only allowed while the request is still pending.</summary>
    public void Update(
        string schoolName,
        string subdomainCode,
        string? address,
        string? phone,
        string? schoolEmail,
        string adminFirstName,
        string adminLastName,
        string adminEmail,
        string? adminPassword,
        string? reviewNotes,
        Guid? subscriptionPlanId = null,
        string? planName = null,
        decimal? planPriceMonthly = null)
    {
        if (Status != SchoolRegistrationRequestStatus.Pending)
            throw new DomainException("Only pending requests can be edited.");

        if (string.IsNullOrWhiteSpace(schoolName))
            throw new DomainException("School name is required.");
        if (string.IsNullOrWhiteSpace(subdomainCode))
            throw new DomainException("Subdomain code is required.");

        SchoolName = schoolName.Trim();
        SubdomainCode = subdomainCode.Trim().ToLowerInvariant();
        Address = address?.Trim();
        Phone = phone?.Trim();
        SchoolEmail = schoolEmail?.Trim();
        AdminFirstName = adminFirstName.Trim();
        AdminLastName = adminLastName.Trim();
        AdminEmail = adminEmail.Trim().ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(adminPassword))
            AdminPassword = adminPassword;
        ReviewNotes = reviewNotes?.Trim();
        if (subscriptionPlanId.HasValue)
        {
            SubscriptionPlanId = subscriptionPlanId;
            PlanName = planName;
            PlanPriceMonthly = planPriceMonthly;
        }
    }

    /// <summary>Approve the request, recording the school and admin account that were created.</summary>
    public void Approve(Guid schoolId, Guid adminUserId, Guid reviewedByUserId, string? notes)
    {
        if (Status != SchoolRegistrationRequestStatus.Pending)
            throw new DomainException("Only pending requests can be approved.");

        Status = SchoolRegistrationRequestStatus.Approved;
        CreatedSchoolId = schoolId;
        CreatedAdminUserId = adminUserId;
        ReviewedByUserId = reviewedByUserId;
        ReviewedAtUtc = DateTime.UtcNow;
        ReviewNotes = notes?.Trim();
    }

    /// <summary>Reject the request with an optional reason.</summary>
    public void Reject(Guid reviewedByUserId, string? reason)
    {
        if (Status != SchoolRegistrationRequestStatus.Pending)
            throw new DomainException("Only pending requests can be rejected.");

        Status = SchoolRegistrationRequestStatus.Rejected;
        ReviewedByUserId = reviewedByUserId;
        ReviewedAtUtc = DateTime.UtcNow;
        ReviewNotes = reason?.Trim();
    }
}
