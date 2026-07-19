using Application.Common.Models;
using MediatR;

namespace Application.Features.Billing.Queries;

public record GetSubscriptionStatusQuery(Guid SchoolId) : IRequest<Result<SubscriptionStatusDto>>;
