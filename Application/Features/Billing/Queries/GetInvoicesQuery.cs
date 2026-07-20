using Application.Common.Models;
using MediatR;

namespace Application.Features.Billing.Queries;

public record GetInvoicesQuery(Guid SchoolId, PaginationParams? Pagination = null)
    : IRequest<Result<PagedResult<InvoiceListDto>>>;
