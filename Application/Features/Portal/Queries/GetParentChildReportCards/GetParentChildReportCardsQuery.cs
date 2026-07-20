using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Portal.Queries.Shared;
using MediatR;

namespace Application.Features.Portal.Queries.GetParentChildReportCards;

public record GetParentChildReportCardsQuery(
    Guid SchoolId,
    Guid StudentId,
    PaginationParams? Pagination = null)
    : IRequest<Result<PagedResult<PortalReportCardDto>>>, ITenantScopedRequest;
