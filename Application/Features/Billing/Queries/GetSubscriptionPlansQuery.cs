using Application.Common.Models;
using MediatR;

namespace Application.Features.Billing.Queries;

public record GetSubscriptionPlansQuery : IRequest<Result<List<SubscriptionPlanDto>>>;
