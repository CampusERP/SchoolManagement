using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.Commands.UpdateGradeLevel;

public class UpdateGradeLevelCommandHandler : IRequestHandler<UpdateGradeLevelCommand, Result>
{
    private readonly IGradeLevelRepository _gradeLevels;

    public UpdateGradeLevelCommandHandler(IGradeLevelRepository gradeLevels)
    {
        _gradeLevels = gradeLevels;
    }

    public async Task<Result> Handle(UpdateGradeLevelCommand request, CancellationToken ct)
    {
        var gradeLevel = await _gradeLevels.GetByIdAsync(request.GradeLevelId, ct);
        if (gradeLevel is null || gradeLevel.SchoolId != request.SchoolId) return Result.Failure("Grade level not found.");

        if (await _gradeLevels.SequenceExistsAsync(request.SchoolId, gradeLevel.EducationStageId, request.Sequence, gradeLevel.Id, ct))
            return Result.Failure($"Sequence {request.Sequence} is already used in this education stage.");

        try
        {
            gradeLevel.Update(request.Name, request.Sequence);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }
}
