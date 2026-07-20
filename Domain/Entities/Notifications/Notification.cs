using Domain.Common;

namespace Domain.Entities.Notifications;

public enum NotificationChannel { InApp, Email, SMS, Push }
public enum NotificationStatus  { Pending, Delivered, Read, Failed }

public class Notification : AuditableEntity
{
    public Guid                 NotificationBatchId { get; private set; }
    public Guid                 RecipientUserId     { get; private set; }
    public NotificationChannel  Channel             { get; private set; }
    public NotificationStatus   Status              { get; private set; }
    public DateTime?            DeliveredAtUtc      { get; private set; }
    public DateTime?            ReadAtUtc           { get; private set; }

    private Notification() { }

    private Notification(Guid id, Guid batchId, Guid recipientUserId,
        NotificationChannel channel) : base(id)
    {
        NotificationBatchId = batchId;
        RecipientUserId     = recipientUserId;
        Channel             = channel;
        Status              = NotificationStatus.Pending;
    }

    internal static Notification Create(Guid batchId, Guid recipientUserId,
        NotificationChannel channel)
        => new(Guid.NewGuid(), batchId, recipientUserId, channel);

    public void MarkDelivered()
    {
        Status         = NotificationStatus.Delivered;
        DeliveredAtUtc = DateTime.UtcNow;
    }

    public void MarkRead()
    {
        if (Status == NotificationStatus.Read) return;
        Status    = NotificationStatus.Read;
        ReadAtUtc = DateTime.UtcNow;
    }

    public void MarkFailed() => Status = NotificationStatus.Failed;
}