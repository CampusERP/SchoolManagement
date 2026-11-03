using Domain.Common;

namespace Domain.Entities.Academics;

/// <summary>
/// Represents a room within a school, which can be used for classes or other activities.
/// </summary>
public class Room : TenantEntity, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public int Capacity { get; private set; }
    public Guid? CampusId { get; private set; }

    private Room() { } // EF Core

    private Room(Guid id, Guid schoolId, string name, int capacity, Guid? campusId) : base(id, schoolId)
    {
        Name = name;
        Capacity = capacity;
        CampusId = campusId;
    }

    public void Update(string name, int capacity, Guid? campusId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be positive.", nameof(capacity));
        Name = name;
        Capacity = capacity;
        CampusId = campusId;
    }

    public static Room Create(Guid schoolId, string name, int capacity, Guid? campusId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be positive.", nameof(capacity));

        return new Room(Guid.NewGuid(), schoolId, name, capacity, campusId);
    }
}
