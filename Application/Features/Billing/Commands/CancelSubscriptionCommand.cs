using Application.Common.Models;
using MediatR;

namespace Application.Features.Billing.Commands;

public record CancelSubscriptionCommand(Guid SchoolId) : ICommand;
