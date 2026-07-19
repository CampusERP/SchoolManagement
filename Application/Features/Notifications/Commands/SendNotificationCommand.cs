using Application.Common.Behaviors;
using Application.Common.Interfaces;
using Domain.Entities.Notifications;

namespace Application.Features.Notifications.Commands;

public record SendNotificationCommand(
    Guid SchoolId,
    string Subject,
    string Body,
    NotificationChannel Channel,
    NotificationScope Scope,
    Guid? TargetUserId    = null,
    Guid? TargetClassRoom = null,
    Guid? TargetGrade     = null,
    Guid? TemplateId      = null)
    : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;
