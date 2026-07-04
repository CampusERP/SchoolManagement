using Domain.Common;

namespace Domain.Entities.Tenancy;

/// <summary>
/// Represents the membership of a user in a school, indicating whether the membership is active or inactive.
/// </summary>
public class UserSchoolMembership : AuditableEntity, IAggregateRoot
{
    public Guid ApplicationUserId { get; private set; }
    public Guid SchoolId { get; private set; }
    public bool IsActive { get; private set; }

    private UserSchoolMembership() { } // EF Core

    private UserSchoolMembership(Guid id, Guid applicationUserId, Guid schoolId) : base(id)
    {
        ApplicationUserId = applicationUserId;
        SchoolId = schoolId;
        IsActive = true;
    }

    public static UserSchoolMembership Create(Guid applicationUserId, Guid schoolId)
    {
        return new UserSchoolMembership(Guid.NewGuid(), applicationUserId, schoolId);
    }

    public void Deactivate() => IsActive = false;
    public void Reactivate() => IsActive = true;
}
