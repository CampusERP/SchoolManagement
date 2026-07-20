namespace Application.Features.Portal.Queries.Shared;

public record PortalNotificationDto(
    Guid     Id,
    string   Subject,
    string   Body,
    string   Status,
    DateTime CreatedAtUtc,
    DateTime? ReadAtUtc);
