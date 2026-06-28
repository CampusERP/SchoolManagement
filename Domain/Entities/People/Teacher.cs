using Domain.Common;
using Domain.Enums;

namespace Domain.Entities.People;

public class Teacher : TenantEntity, IAggregateRoot
{
    public Guid ApplicationUserId { get; private set; }
    public string EmployeeCode { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public EmploymentStatus EmploymentStatus { get; private set; }

    private Teacher() { } // EF Core

    private Teacher(Guid id, Guid schoolId, Guid applicationUserId, string employeeCode,
        string firstName, string lastName) : base(id, schoolId)
    {
        ApplicationUserId = applicationUserId;
        EmployeeCode = employeeCode;
        FirstName = firstName;
        LastName = lastName;
        EmploymentStatus = EmploymentStatus.Active;
    }

    public static Teacher Create(Guid schoolId, Guid applicationUserId, string employeeCode,
        string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(employeeCode))
            throw new ArgumentException("Employee code is required.", nameof(employeeCode));

        return new Teacher(Guid.NewGuid(), schoolId, applicationUserId, employeeCode, firstName, lastName);
    }

    public void SetOnLeave() => EmploymentStatus = EmploymentStatus.OnLeave;
    public void Terminate() => EmploymentStatus = EmploymentStatus.Terminated;
    public void Reactivate() => EmploymentStatus = EmploymentStatus.Active;
}
