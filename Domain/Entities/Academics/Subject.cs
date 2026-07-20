using Domain.Common;

namespace Domain.Entities.Academics;

/// <summary>
/// Global lookup — deliberately not tenant-scoped (no SchoolId) so that
/// cross-school reporting ("Math attendance rate across all schools") stays
/// possible without joining on free-text strings. Schools cannot create
/// arbitrary Subject names; they choose from this list via CurriculumSubject.
/// If a school truly needs a custom subject, that's a Phase 3 extension table.
/// </summary>
public class Subject : Entity
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }

    private Subject() { } // EF Core

    private Subject(Guid id, string code, string name, string? description) : base(id)
    {
        Code = code;
        Name = name;
        Description = description;
    }

    public static Subject Create(string code, string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))  throw new ArgumentException("Code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))  throw new ArgumentException("Name is required.", nameof(name));

        return new Subject(Guid.NewGuid(), code.ToUpperInvariant(), name, description);
    }
}
