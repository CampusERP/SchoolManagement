using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Billing.Queries;

public class GetInvoiceByIdQueryHandler
    : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDetailDto>>
{
    private readonly IBillingReadService _readService;
    public GetInvoiceByIdQueryHandler(IBillingReadService readService) => _readService = readService;

    public async Task<Result<InvoiceDetailDto>> Handle(
        GetInvoiceByIdQuery request, CancellationToken ct)
    {
        var invoice = await _readService.GetInvoiceByIdAsync(request.InvoiceId, ct);
        if (invoice is null)
            return Result.Failure<InvoiceDetailDto>("Invoice not found.");

        return Result.Success(invoice);
    }
}
