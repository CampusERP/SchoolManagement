using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Entities.Billing;
using MediatR;

namespace Application.Features.Billing.Commands;

public class RecordPaymentCommandHandler
    : IRequestHandler<RecordPaymentCommand, Result<Guid>>
{
    private readonly IInvoiceRepository _invoiceRepository;
    public RecordPaymentCommandHandler(IInvoiceRepository invoiceRepository)
        => _invoiceRepository = invoiceRepository;

    public async Task<Result<Guid>> Handle(RecordPaymentCommand request, CancellationToken ct)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, ct);
        if (invoice is null)
            throw new NotFoundException(nameof(Invoice), request.InvoiceId);

        try
        {
            var payment = invoice.RecordPayment(request.Amount, request.Method, request.Reference);
            _invoiceRepository.Update(invoice);
            return Result.Success(payment.Id);
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return Result.Failure<Guid>(ex.Message);
        }
    }
}
