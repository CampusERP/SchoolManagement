using Domain.Common;

namespace Domain.Entities.Academics;

/// <summary>
/// Global lookup, NOT tenant-scoped (no SchoolId) — deliberately shared
/// across all schools so cross-school reporting/analytics stays possible.
/// Seeded once (Primary / Middle / Secondary), not created by schools.
/// </summary>
public class EducationStage : Entity
{
    public string Name { get; private set; } = default!;

    private EducationStage() { } // EF Core

    private EducationStage(Guid id, string name) : base(id)
    {
        Name = name;
    }

    public static EducationStage Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        return new EducationStage(Guid.NewGuid(), name);
    }
}
