using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Portal.Queries.GetParentDashboard;

public record GetParentDashboardQuery(Guid SchoolId)
    : IRequest<Result<ParentDashboardDto>>, ITenantScopedRequest;
