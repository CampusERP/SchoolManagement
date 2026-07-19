using Application.Common.Models;
using MediatR;

namespace Application.Features.Billing.Commands;

public record SuspendSubscriptionCommand(Guid SchoolId) : ICommand;
