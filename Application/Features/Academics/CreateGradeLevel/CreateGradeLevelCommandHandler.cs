using MediatR;
using Application.Common.Models;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.Academics;

namespace Application.Features.Academics.CreateGradeLevel;

public class CreateGradeLevelCommandHandler : IRequestHandler<CreateGradeLevelCommand, Result<Guid>>
{
    private readonly IGradeLevelRepository _gradeLevels;

    public CreateGradeLevelCommandHandler(IGradeLevelRepository gradeLevels)
    {
        _gradeLevels = gradeLevels;
    }

    public async Task<Result<Guid>> Handle(CreateGradeLevelCommand request, CancellationToken ct)
    {

        var gradeLevel = GradeLevel.Create(request.SchoolId, request.EducationStageId, request.Name, request.Sequence);
        await _gradeLevels.AddAsync(gradeLevel, ct);

        return Result.Success(gradeLevel.Id);
    }
}