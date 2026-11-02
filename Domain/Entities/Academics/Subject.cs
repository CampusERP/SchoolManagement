using Domain.Common;

namespace Domain.Entities.Academics;

/// <summary>Shared catalog subject or school-owned custom subject.</summary>
public class Subject : Entity
{
    public Guid? SchoolId { get; private set; }
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }

    private Subject() { }

    private Subject(Guid id, Guid? schoolId, string code, string name, string? description) : base(id)
    {
        SchoolId = schoolId;
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Description = Normalize(description);
    }

    public static Subject Create(string code, string name, string? description = null) =>
        Create(null, code, name, description);

    public static Subject Create(Guid? schoolId, string code, string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        return new Subject(Guid.NewGuid(), schoolId, code, name, description);
    }

    public void Update(string code, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Description = Normalize(description);
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
