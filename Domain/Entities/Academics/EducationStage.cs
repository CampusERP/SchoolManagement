using Domain.Common;

namespace Domain.Entities.Academics;

/// <summary>
/// Represents an education stage in the academic system, such as primary, secondary, or tertiary education.
/// </summary>
public class EducationStage : Entity
{
    public string Name { get; private set; } = default!;

    private EducationStage() { } // EF Core

    private EducationStage(Guid id, string name) : base(id)
    {
        Name = name;
    }

    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        Name = name;
    }

    public static EducationStage Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        return new EducationStage(Guid.NewGuid(), name);
    }
}
