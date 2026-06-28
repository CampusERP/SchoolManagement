using Domain.Common;

namespace Domain.Entities.Academics;

/// <summary>
/// Physical room/location. Without this, ClassSchedule clash detection
/// ("two classes booked in the same room") is impossible.
/// </summary>
public class Room : TenantEntity, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public int Capacity { get; private set; }

    private Room() { } // EF Core

    private Room(Guid id, Guid schoolId, string name, int capacity) : base(id, schoolId)
    {
        Name = name;
        Capacity = capacity;
    }

    public static Room Create(Guid schoolId, string name, int capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be positive.", nameof(capacity));

        return new Room(Guid.NewGuid(), schoolId, name, capacity);
    }
}
