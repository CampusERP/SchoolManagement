using Application.Common.Models;
using MediatR;

namespace Application.Features.Schools.Queries.GetPlatformAnalytics;

public record GetPlatformAnalyticsQuery : IRequest<Result<PlatformAnalyticsDto>>;
