using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Notifications.Queries;

public record GetUnreadNotificationCountQuery(Guid SchoolId)
    : IRequest<Result<int>>, ITenantScopedRequest;
