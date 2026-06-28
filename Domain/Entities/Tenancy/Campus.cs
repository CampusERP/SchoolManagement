using Domain.Common;

namespace Domain.Entities.Tenancy;

/// <summary>
/// A physical campus/location belonging to a School. Child of the School
/// aggregate — created only through School.AddCampus, never directly.
/// </summary>
public class Campus : AuditableEntity
{
    public Guid SchoolId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Address { get; private set; } = default!;

    private Campus() { } // EF Core

    private Campus(Guid id, Guid schoolId, string name, string address) : base(id)
    {
        SchoolId = schoolId;
        Name = name;
        Address = address;
    }

    internal static Campus Create(Guid schoolId, string name, string address)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Campus name is required.", nameof(name));

        return new Campus(Guid.NewGuid(), schoolId, name, address);
    }
}
