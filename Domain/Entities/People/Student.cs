using Domain.Common;

namespace Domain.Entities.People;

/// <summary>
/// Represents a student entity in the school management system.
/// </summary>
public class Student : TenantEntity, IAggregateRoot
{
    public Guid? ApplicationUserId { get; private set; }
    public bool LoginEnabled { get; private set; } = true;
    public bool HasLogin => ApplicationUserId.HasValue && LoginEnabled;
    public string StudentCode { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string? SecondName { get; private set; }
    public string? ThirdName { get; private set; }
    public string LastName { get; private set; } = default!;
    public string? NationalityCountry { get; private set; }
    public string? ResidenceCountry { get; private set; }
    public Guid? CampusId { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public string? NationalId { get; private set; }

    private Student() { } // EF Core

    private Student(Guid id, Guid schoolId, string studentCode, string firstName,
        string lastName, DateTime dateOfBirth, string? nationalId, string? secondName,
        string? thirdName, string? nationalityCountry, string? residenceCountry, Guid? campusId) : base(id, schoolId)
    {
        StudentCode = studentCode;
        FirstName = firstName;
        SecondName = Normalize(secondName);
        ThirdName = Normalize(thirdName);
        LastName = lastName;
        NationalityCountry = Normalize(nationalityCountry);
        ResidenceCountry = Normalize(residenceCountry);
        CampusId = campusId;
        DateOfBirth = dateOfBirth;
        NationalId = nationalId;
    }

    public static Student Create(Guid schoolId, string studentCode, string firstName,
        string lastName, DateTime dateOfBirth, string? nationalId = null,
        string? secondName = null, string? thirdName = null,
        string? nationalityCountry = null, string? residenceCountry = null, Guid? campusId = null)
    {
        if (string.IsNullOrWhiteSpace(studentCode))
            throw new ArgumentException("Student code is required.", nameof(studentCode));
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("First and last name are required.");

        return new Student(Guid.NewGuid(), schoolId, studentCode, firstName.Trim(), lastName.Trim(), dateOfBirth, nationalId,
            secondName, thirdName, nationalityCountry, residenceCountry, campusId);
    }

    public void Update(string studentCode, string firstName, string lastName, DateTime dateOfBirth, string? nationalId = null,
        string? secondName = null, string? thirdName = null,
        string? nationalityCountry = null, string? residenceCountry = null, Guid? campusId = null)
    {
        if (string.IsNullOrWhiteSpace(studentCode))
            throw new ArgumentException("Student code is required.", nameof(studentCode));
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("First and last name are required.");
        if (dateOfBirth.Date >= DateTime.UtcNow.Date)
            throw new ArgumentException("Date of birth must be in the past.", nameof(dateOfBirth));

        StudentCode = studentCode;
        FirstName = firstName;
        SecondName = Normalize(secondName);
        ThirdName = Normalize(thirdName);
        LastName = lastName;
        NationalityCountry = Normalize(nationalityCountry);
        ResidenceCountry = Normalize(residenceCountry);
        CampusId = campusId;
        DateOfBirth = dateOfBirth;
        NationalId = nationalId;
    }

    public void LinkLogin(Guid applicationUserId)
    {
        ApplicationUserId = applicationUserId;
    }

    public void SetLoginEnabled(bool enabled)
    {
        if (!ApplicationUserId.HasValue && enabled)
            throw new InvalidOperationException("A login account must be linked before it can be enabled.");
        LoginEnabled = enabled;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
