using Domain.Common;

namespace Domain.Entities.Notifications;

public class DeviceToken : AuditableEntity, IAggregateRoot
{
    public Guid   ApplicationUserId { get; private set; }
    public string Platform          { get; private set; } = default!; // iOS, Android, Web
    public string Token             { get; private set; } = default!;
    public DateTime LastSeenUtc     { get; private set; }

    private DeviceToken() { }

    private DeviceToken(Guid id, Guid userId, string platform, string token) : base(id)
    {
        ApplicationUserId = userId;
        Platform          = platform;
        Token             = token;
        LastSeenUtc       = DateTime.UtcNow;
    }

    public static DeviceToken Create(Guid userId, string platform, string token)
        => new(Guid.NewGuid(), userId, platform, token);

    public void RefreshLastSeen() => LastSeenUtc = DateTime.UtcNow;
}
