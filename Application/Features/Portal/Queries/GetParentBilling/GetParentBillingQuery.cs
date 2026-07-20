using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Portal.Queries.Shared;
using MediatR;

namespace Application.Features.Portal.Queries.GetParentBilling;

public record GetParentBillingQuery(
    Guid SchoolId,
    PaginationParams? Pagination = null)
    : IRequest<Result<PagedResult<PortalInvoiceDto>>>, ITenantScopedRequest;
