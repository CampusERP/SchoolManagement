using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.People.Queries.GetParents;

public record GetParentsQuery(Guid SchoolId, string? SearchTerm = null,
    PaginationParams? Pagination = null)
    : IRequest<Result<PagedResult<ParentListDto>>>, ITenantScopedRequest;
