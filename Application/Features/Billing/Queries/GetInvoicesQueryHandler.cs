using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Billing.Queries;

public class GetInvoicesQueryHandler
    : IRequestHandler<GetInvoicesQuery, Result<PagedResult<InvoiceListDto>>>
{
    private readonly IBillingReadService _readService;
    public GetInvoicesQueryHandler(IBillingReadService readService) => _readService = readService;

    public async Task<Result<PagedResult<InvoiceListDto>>> Handle(
        GetInvoicesQuery request, CancellationToken ct)
    {
        var p = request.Pagination ?? new PaginationParams();
        var result = await _readService.GetInvoicesAsync(p, request.SchoolId, ct);
        return Result.Success(result);
    }
}
