using Domain.Common;

namespace Domain.Entities.Academics;

/// <summary>
/// Represents a classroom within a school, associated with a specific grade level and academic year.
/// </summary>
public class ClassRoom : TenantEntity, IAggregateRoot
{
    public Guid GradeLevelId { get; private set; }
    public Guid AcademicYearId { get; private set; }
    public Guid? RoomId { get; private set; }
    public string Name { get; private set; } = default!;

    private ClassRoom() { } // EF Core

    private ClassRoom(Guid id, Guid schoolId, Guid gradeLevelId, Guid academicYearId, string name, Guid? roomId)
        : base(id, schoolId)
    {
        GradeLevelId = gradeLevelId;
        AcademicYearId = academicYearId;
        Name = name;
        RoomId = roomId;
    }

    public void Update(string name, Guid? roomId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        Name = name;
        RoomId = roomId;
    }

    public static ClassRoom Create(Guid schoolId, Guid gradeLevelId, Guid academicYearId, string name, Guid? roomId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        return new ClassRoom(Guid.NewGuid(), schoolId, gradeLevelId, academicYearId, name, roomId);
    }
}
