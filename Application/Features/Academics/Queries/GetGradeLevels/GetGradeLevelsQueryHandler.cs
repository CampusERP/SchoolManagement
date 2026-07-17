using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Queries.GetGradeLevels;

public class GetGradeLevelsQueryHandler
    : IRequestHandler<GetGradeLevelsQuery, Result<List<GradeLevelDto>>>
{
    private readonly IAcademicReadService _academicReadService;
    public GetGradeLevelsQueryHandler(IAcademicReadService academicReadService) => _academicReadService = academicReadService;

    public async Task<Result<List<GradeLevelDto>>> Handle(
        GetGradeLevelsQuery request, CancellationToken ct)
    {
        var items = await _academicReadService.GetGradeLevelsAsync(ct);
        return Result.Success(items);
    }
}
