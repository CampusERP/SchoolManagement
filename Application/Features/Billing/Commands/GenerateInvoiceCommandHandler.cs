using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Billing.Commands;

public class GenerateInvoiceCommandHandler
    : IRequestHandler<GenerateInvoiceCommand, Result<Guid>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    public GenerateInvoiceCommandHandler(ISubscriptionRepository subscriptionRepository)
        => _subscriptionRepository = subscriptionRepository;

    public async Task<Result<Guid>> Handle(GenerateInvoiceCommand request, CancellationToken ct)
    {
        var sub = await _subscriptionRepository.GetActiveBySchoolIdAsync(request.SchoolId, ct);
        if (sub is null)
            return Result.Failure<Guid>("No active subscription found for this school.");

        try
        {
            var invoice = sub.GenerateInvoice(request.Amount, request.DueDate);
            _subscriptionRepository.Update(sub);
            return Result.Success(invoice.Id);
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return Result.Failure<Guid>(ex.Message);
        }
    }
}
