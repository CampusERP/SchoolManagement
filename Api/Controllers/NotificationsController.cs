using Application.Common.Models;
using Application.Features.Notifications.Commands;
using Application.Features.Notifications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
public sealed class NotificationsController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Notification.Read")]
    public async Task<IActionResult> GetMine(Guid schoolId, bool unreadOnly = false, int page = 1, int pageSize = 20, CancellationToken ct = default) => FromResult(await mediator.Send(new GetMyNotificationsQuery(schoolId, unreadOnly, new PaginationParams(page, pageSize)), ct));

    [HttpGet("unread-count")]
    [Authorize(Policy = "Notification.Read")]
    public async Task<IActionResult> GetUnreadCount(Guid schoolId, CancellationToken ct) => FromResult(await mediator.Send(new GetUnreadNotificationCountQuery(schoolId), ct));

    [HttpPost]
    [Authorize(Policy = "Notification.Send")]
    public async Task<IActionResult> Send(SendNotificationCommand command, CancellationToken ct) => Created(await mediator.Send(command, ct));

    [HttpPost("device-tokens")]
    public async Task<IActionResult> RegisterDevice(RegisterDeviceTokenCommand command, CancellationToken ct) => FromResult(await mediator.Send(command, ct));

    [HttpPatch("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid notificationId, Guid schoolId, CancellationToken ct) => FromResult(await mediator.Send(new MarkNotificationReadCommand(schoolId, notificationId), ct));

    [HttpPatch("read")]
    public async Task<IActionResult> MarkAllRead(Guid schoolId, CancellationToken ct) => FromResult(await mediator.Send(new MarkAllNotificationsReadCommand(schoolId), ct));
}
