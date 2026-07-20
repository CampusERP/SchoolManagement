namespace Application.Features.Notifications.Queries;

public record NotificationDto(
    Guid   Id,
    string Subject,
    string Body,
    string Channel,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? ReadAtUtc);
