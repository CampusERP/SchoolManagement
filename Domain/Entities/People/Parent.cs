using Domain.Common;

namespace Domain.Entities.People;

public class Parent : TenantEntity, IAggregateRoot
{
    public Guid ApplicationUserId { get; private set; }
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;

    private Parent() { } // EF Core

    private Parent(Guid id, Guid schoolId, Guid applicationUserId, string firstName, string lastName)
        : base(id, schoolId)
    {
        ApplicationUserId = applicationUserId;
        FirstName = firstName;
        LastName = lastName;
    }

    public static Parent Create(Guid schoolId, Guid applicationUserId, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("First and last name are required.");

        return new Parent(Guid.NewGuid(), schoolId, applicationUserId, firstName, lastName);
    }

    public void Update(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("First and last name are required.");

        FirstName = firstName;
        LastName = lastName;
    }
}
