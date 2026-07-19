using Application.Common.Models;
using Domain.Entities.Billing;
using MediatR;

namespace Application.Features.Billing.Commands;

public record RecordPaymentCommand(
    Guid InvoiceId, decimal Amount, PaymentMethod Method, string? Reference = null)
    : ICommand<Guid>;
