using Domain.Common;

namespace Domain.Entities.Notifications;
public class NotificationBatch : TenantEntity, IAggregateRoot
{
    public Guid?                NotificationTemplateId { get; private set; }
    public Guid                 TriggeredByUserId      { get; private set; }
    public string               Subject                { get; private set; } = default!;
    public string               Body                   { get; private set; } = default!;
    public NotificationChannel  Channel                { get; private set; }
    public string               ScopeDescription       { get; private set; } = default!;

    private readonly List<Notification> _notifications = new();
    public IReadOnlyCollection<Notification> Notifications => _notifications.AsReadOnly();

    private NotificationBatch() { }

    private NotificationBatch(Guid id, Guid schoolId, Guid? templateId, Guid triggeredBy,
        string subject, string body, NotificationChannel channel, string scope) : base(id, schoolId)
    {
        NotificationTemplateId = templateId;
        TriggeredByUserId      = triggeredBy;
        Subject                = subject;
        Body                   = body;
        Channel                = channel;
        ScopeDescription       = scope;
    }

    public static NotificationBatch Create(Guid schoolId, Guid? templateId, Guid triggeredBy,
        string subject, string body, NotificationChannel channel, string scope)
        => new(Guid.NewGuid(), schoolId, templateId, triggeredBy, subject, body, channel, scope);

    public Notification AddRecipient(Guid recipientUserId)
    {
        var notification = Notification.Create(Id, recipientUserId, Channel);
        _notifications.Add(notification);
        return notification;
    }
}
