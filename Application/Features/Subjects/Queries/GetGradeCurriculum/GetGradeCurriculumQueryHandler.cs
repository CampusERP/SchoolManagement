using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Subjects.Queries.GetGradeCurriculum;

public sealed class GetGradeCurriculumQueryHandler : IRequestHandler<GetGradeCurriculumQuery, Result<List<CurriculumSubjectDto>>>
{
    private readonly ISubjectReadService _subjects;
    public GetGradeCurriculumQueryHandler(ISubjectReadService subjects) => _subjects = subjects;

    public async Task<Result<List<CurriculumSubjectDto>>> Handle(GetGradeCurriculumQuery request, CancellationToken ct)
    {
        var curriculum = await _subjects.GetGradeCurriculumAsync(request.SchoolId, request.GradeLevelId, ct);
        return Result.Success(curriculum);
    }
}
