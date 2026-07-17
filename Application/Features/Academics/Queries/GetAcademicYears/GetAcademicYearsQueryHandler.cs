using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Queries.GetAcademicYears;

public class GetAcademicYearsQueryHandler
    : IRequestHandler<GetAcademicYearsQuery, Result<List<AcademicYearDto>>>
{
    private readonly IAcademicReadService _academicReadService;
    public GetAcademicYearsQueryHandler(IAcademicReadService academicReadService) => _academicReadService = academicReadService;

    public async Task<Result<List<AcademicYearDto>>> Handle(
        GetAcademicYearsQuery request, CancellationToken ct)
    {
        var items = await _academicReadService.GetAcademicYearsAsync(ct);
        return Result.Success(items);
    }
}
