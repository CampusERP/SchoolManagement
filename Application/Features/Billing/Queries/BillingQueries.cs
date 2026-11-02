using Application.Common.Models;
using MediatR;

namespace Application.Features.Billing.Queries;

public record GetSubscriptionPlansQuery(bool ActiveOnly = false)
    : IRequest<Result<List<SubscriptionPlanDto>>>;

public record GetSubscriptionsQuery(
    PaginationParams? Pagination = null,
    Guid? SchoolId = null,
    string? Search = null)
    : IRequest<Result<PagedResult<SubscriptionStatusDto>>>;

public record GetInvoicesQuery(
    Guid? SchoolId = null,
    PaginationParams? Pagination = null,
    string? Search = null,
    string? Status = null)
    : IRequest<Result<PagedResult<InvoiceListDto>>>;

public record GetInvoiceByIdQuery(Guid InvoiceId)
    : IRequest<Result<InvoiceDetailDto>>;
