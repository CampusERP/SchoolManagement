using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Notifications.Commands;

public record MarkNotificationReadCommand(Guid SchoolId, Guid NotificationId)
    : ICommand, IBaseCommand, ITenantScopedRequest;
