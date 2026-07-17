using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Schools.Queries.GetAllSchools;

public class GetAllSchoolsQueryHandler
    : IRequestHandler<GetAllSchoolsQuery, Result<PagedResult<SchoolListDto>>>
{
    private readonly ISchoolReadService _schoolReadService;
    public GetAllSchoolsQueryHandler(ISchoolReadService schoolReadService) => _schoolReadService = schoolReadService;

    public async Task<Result<PagedResult<SchoolListDto>>> Handle(
        GetAllSchoolsQuery request, CancellationToken ct)
    {
        var p = request.Pagination ?? new PaginationParams();
        var result = await _schoolReadService.GetSchoolsAsync(p, ct);
        return Result.Success(result);
    }
}
