using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Notifications.Queries;

public record GetMyNotificationsQuery(Guid SchoolId,
    bool UnreadOnly = false, PaginationParams? Pagination = null)
    : IRequest<Result<PagedResult<NotificationDto>>>, ITenantScopedRequest;
