using Domain.Common;

namespace Domain.Entities.Academics;

/// <summary>
/// Represents a classroom within a school, associated with a specific grade level and academic year.
/// </summary>
public class ClassRoom : TenantEntity, IAggregateRoot
{
    public Guid GradeLevelId { get; private set; }
    public Guid AcademicYearId { get; private set; }
    public string Name { get; private set; } = default!;

    private ClassRoom() { } // EF Core

    private ClassRoom(Guid id, Guid schoolId, Guid gradeLevelId, Guid academicYearId, string name)
        : base(id, schoolId)
    {
        GradeLevelId = gradeLevelId;
        AcademicYearId = academicYearId;
        Name = name;
    }

    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        Name = name;
    }

    public static ClassRoom Create(Guid schoolId, Guid gradeLevelId, Guid academicYearId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        return new ClassRoom(Guid.NewGuid(), schoolId, gradeLevelId, academicYearId, name);
    }
}
