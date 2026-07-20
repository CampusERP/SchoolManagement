using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Notifications.Commands;

public record MarkAllNotificationsReadCommand(Guid SchoolId)
    : ICommand, IBaseCommand, ITenantScopedRequest;
