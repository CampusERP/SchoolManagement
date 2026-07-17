using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Academics.UpdateGradeLevel;

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
        if (gradeLevel is null) return Result.Failure("Grade level not found.");

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
