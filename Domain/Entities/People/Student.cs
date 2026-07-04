using Domain.Common;

namespace Domain.Entities.People;

/// <summary>
/// Represents a student entity in the school management system.
/// </summary>
public class Student : TenantEntity, IAggregateRoot
{
    public Guid? ApplicationUserId { get; private set; }
    public string StudentCode { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public DateTime DateOfBirth { get; private set; }

    private Student() { } // EF Core

    private Student(Guid id, Guid schoolId, string studentCode, string firstName,
        string lastName, DateTime dateOfBirth) : base(id, schoolId)
    {
        StudentCode = studentCode;
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
    }

    public static Student Create(Guid schoolId, string studentCode, string firstName,
        string lastName, DateTime dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(studentCode))
            throw new ArgumentException("Student code is required.", nameof(studentCode));
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("First and last name are required.");

        return new Student(Guid.NewGuid(), schoolId, studentCode, firstName, lastName, dateOfBirth);
    }

    public void LinkLogin(Guid applicationUserId) => ApplicationUserId = applicationUserId;
}
