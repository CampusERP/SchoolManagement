using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Portal.Queries.Shared;
using MediatR;

namespace Application.Features.Portal.Queries.GetParentChildGrades;

public record GetParentChildGradesQuery(
    Guid SchoolId,
    Guid StudentId,
    Guid? TermId = null,
    PaginationParams? Pagination = null)
    : IRequest<Result<PagedResult<PortalExamResultDto>>>, ITenantScopedRequest;
