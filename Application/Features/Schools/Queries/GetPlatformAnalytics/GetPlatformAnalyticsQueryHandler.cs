using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Schools.Queries.GetPlatformAnalytics;

public class GetPlatformAnalyticsQueryHandler
    : IRequestHandler<GetPlatformAnalyticsQuery, Result<PlatformAnalyticsDto>>
{
    private readonly ISchoolReadService _schoolReadService;
    public GetPlatformAnalyticsQueryHandler(ISchoolReadService schoolReadService) => _schoolReadService = schoolReadService;

    public async Task<Result<PlatformAnalyticsDto>> Handle(
        GetPlatformAnalyticsQuery request, CancellationToken ct)
    {
        var dto = await _schoolReadService.GetPlatformAnalyticsAsync(ct);
        return Result.Success(dto);
    }
}
