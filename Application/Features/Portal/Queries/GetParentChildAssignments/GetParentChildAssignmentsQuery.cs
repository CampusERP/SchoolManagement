using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Portal.Queries.Shared;
using MediatR;

namespace Application.Features.Portal.Queries.GetParentChildAssignments;

public record GetParentChildAssignmentsQuery(
    Guid SchoolId,
    Guid StudentId,
    PaginationParams? Pagination = null)
    : IRequest<Result<PagedResult<PortalAssignmentDto>>>, ITenantScopedRequest;
