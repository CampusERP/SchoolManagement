using MediatR;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Domain.Entities.Academics;

namespace Application.Features.Academics.Queries.GetEducationStages;

public class GetEducationStagesQueryHandler
    : IRequestHandler<GetEducationStagesQuery, Result<List<EducationStageDto>>>
{
    private readonly IEducationStageRepository _educationStages;

    public GetEducationStagesQueryHandler(IEducationStageRepository educationStages)
    {
        _educationStages = educationStages;
    }

    public async Task<Result<List<EducationStageDto>>> Handle(
        GetEducationStagesQuery request, CancellationToken ct)
    {
        var stages = await _educationStages.GetAllAsync(ct);
        var dtos = stages.Select(s => new EducationStageDto(s.Id, s.Name)).ToList();
        return Result.Success(dtos);
    }
}
