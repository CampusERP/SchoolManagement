using Domain.Common;

namespace Domain.Entities.People;

public class SchoolAdminProfile : TenantEntity, IAggregateRoot
{
    public Guid ApplicationUserId { get; private set; }
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;

    private SchoolAdminProfile() { } // EF Core

    private SchoolAdminProfile(Guid id, Guid schoolId, Guid applicationUserId,
        string firstName, string lastName) : base(id, schoolId)
    {
        ApplicationUserId = applicationUserId;
        FirstName = firstName;
        LastName = lastName;
    }

    public static SchoolAdminProfile Create(Guid schoolId, Guid applicationUserId,
        string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("First and last name are required.");

        return new SchoolAdminProfile(Guid.NewGuid(), schoolId, applicationUserId, firstName, lastName);
    }
}
