using Application.Common.Models;
using MediatR;

namespace Application.Features.Billing.Commands;

public record GenerateInvoiceCommand(Guid SchoolId, decimal Amount, DateTime DueDate)
    : ICommand<Guid>;
