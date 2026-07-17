using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Schools.Queries.GetSchoolById;

public class GetSchoolByIdQueryHandler
    : IRequestHandler<GetSchoolByIdQuery, Result<SchoolDetailDto>>
{
    private readonly ISchoolReadService _schoolReadService;
    public GetSchoolByIdQueryHandler(ISchoolReadService schoolReadService) => _schoolReadService = schoolReadService;

    public async Task<Result<SchoolDetailDto>> Handle(
        GetSchoolByIdQuery request, CancellationToken ct)
    {
        var school = await _schoolReadService.GetSchoolByIdAsync(request.SchoolId, ct);
        if (school is null) return Result.Failure<SchoolDetailDto>("School not found.");
        return Result.Success(school);
    }
}
