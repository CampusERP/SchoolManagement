using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Schools.Queries.GetSchoolDashboard;

public class GetSchoolDashboardQueryHandler
    : IRequestHandler<GetSchoolDashboardQuery, Result<SchoolDashboardDto>>
{
    private readonly ISchoolReadService _schoolReadService;

    public GetSchoolDashboardQueryHandler(ISchoolReadService schoolReadService)
    {
        _schoolReadService = schoolReadService;
    }

    public async Task<Result<SchoolDashboardDto>> Handle(
        GetSchoolDashboardQuery request, CancellationToken ct)
    {
        var dto = await _schoolReadService.GetSchoolDashboardAsync(request.SchoolId, ct);
        if (dto is null) return Result.Failure<SchoolDashboardDto>("School not found.");
        return Result.Success(dto);
    }
}
