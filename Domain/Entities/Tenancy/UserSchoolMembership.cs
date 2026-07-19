using Domain.Common;
using Domain.Exceptions;

namespace Domain.Entities.Tenancy;

/// <summary>
/// Represents the membership of a user in a school, indicating whether the membership is active or inactive.
/// </summary>
public class UserSchoolMembership : AuditableEntity, IAggregateRoot
{
    public Guid ApplicationUserId { get; private set; }
    public Guid SchoolId { get; private set; }
    public string Role { get; private set; }
    public bool IsActive { get; private set; }

    private UserSchoolMembership() { } // EF Core

    private UserSchoolMembership(Guid id, Guid applicationUserId, Guid schoolId, string role) : base(id)
    {
        ApplicationUserId = applicationUserId;
        SchoolId = schoolId;
        Role = role;
        IsActive = true;
    }

    public static UserSchoolMembership Create(Guid applicationUserId, Guid schoolId, string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new DomainException("Role is required for a school membership.");

        return new UserSchoolMembership(Guid.NewGuid(), applicationUserId, schoolId, role);
    }

    public void Deactivate() => IsActive = false;
    public void Reactivate() => IsActive = true;
    public void ChangeRole(string newRole)
    {
        if (string.IsNullOrWhiteSpace(newRole))
            throw new DomainException("Role cannot be empty.");
        Role = newRole;
    }
}