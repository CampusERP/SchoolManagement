using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Portal.Queries.Shared;
using MediatR;

namespace Application.Features.Portal.Queries.GetStudentNotifications;

public record GetStudentNotificationsQuery(
    Guid SchoolId,
    int  Limit = 20)
    : IRequest<Result<List<PortalNotificationDto>>>, ITenantScopedRequest;
