using MediatR;
using Application.Common.Interfaces;
using Application.Common.Models;

namespace Application.Features.Assignments.Queries.GetClassAssignments;

public record GetClassAssignmentsQuery(
    Guid SchoolId,
    Guid TeachingAssignmentId,
    PaginationParams? Pagination = null)
    : IRequest<Result<PagedResult<AssignmentSummaryDto>>>, ITenantScopedRequest;
