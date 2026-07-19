using Domain.Common;

namespace Domain.Entities.Notifications;

public class NotificationTemplate : TenantEntity, IAggregateRoot
{
    public string              Type            { get; private set; } = default!;
    public NotificationChannel Channel         { get; private set; }
    public string              SubjectTemplate { get; private set; } = default!;
    public string              BodyTemplate    { get; private set; } = default!;

    private NotificationTemplate() { }

    private NotificationTemplate(Guid id, Guid schoolId, string type,
        NotificationChannel channel, string subject, string body) : base(id, schoolId)
    {
        Type            = type;
        Channel         = channel;
        SubjectTemplate = subject;
        BodyTemplate    = body;
    }

    public static NotificationTemplate Create(Guid schoolId, string type,
        NotificationChannel channel, string subject, string body)
        => new(Guid.NewGuid(), schoolId, type, channel, subject, body);
}
