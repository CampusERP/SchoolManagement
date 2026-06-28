using Domain.Common;

namespace Domain.Entities.Tenancy;

/// <summary>
/// Links an ApplicationUser to a School. A user can have zero, one, or
/// many memberships (multi-school teachers, Super Admins with none).
/// This is what makes the JWT's school_id claim possible at login —
/// the user picks (or the system infers) which membership is active.
/// Lives in PlatformDbContext alongside Identity tables.
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
